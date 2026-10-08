-- GATE 5 — Command system + TRAVEL
-- All mutations go through validated server commands.
-- request_id provides idempotency.
-- Every successful mutation emits a domain event.

-- ---------------------------------------------------------------------------
-- Seed first-night locations (Apartment → Suya Spot)
-- ---------------------------------------------------------------------------
insert into public.locations (id, name, type)
values
    ('a1111111-1111-1111-1111-111111111101', 'Apartment', 'APARTMENT'),
    ('a1111111-1111-1111-1111-111111111102', 'Suya Spot', 'SUYA_SPOT')
on conflict (id) do nothing;

-- ---------------------------------------------------------------------------
-- Player current location (authoritative presence)
-- ---------------------------------------------------------------------------
create table if not exists public.player_locations (
    player_id uuid primary key references public.players(id) on delete cascade,
    location_id uuid not null references public.locations(id),
    updated_at timestamptz not null default now()
);

create index if not exists player_locations_location_id_idx
    on public.player_locations (location_id);

alter table public.player_locations enable row level security;

create policy "player_locations_read_own"
    on public.player_locations
    for select
    to authenticated
    using (player_id = auth.uid());

-- Default new players to Apartment
create or replace function public.set_default_player_location()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
begin
    insert into public.player_locations (player_id, location_id)
    values (new.id, 'a1111111-1111-1111-1111-111111111101')
    on conflict (player_id) do nothing;
    return new;
end;
$$;

drop trigger if exists on_player_created_set_location on public.players;
create trigger on_player_created_set_location
    after insert on public.players
    for each row
    execute function public.set_default_player_location();

-- Backfill existing players
insert into public.player_locations (player_id, location_id)
select p.id, 'a1111111-1111-1111-1111-111111111101'
from public.players p
where not exists (
    select 1 from public.player_locations pl where pl.player_id = p.id
)
on conflict (player_id) do nothing;

-- ---------------------------------------------------------------------------
-- Idempotency receipts
-- ---------------------------------------------------------------------------
create table if not exists public.command_receipts (
    request_id text primary key,
    player_id uuid not null references public.players(id) on delete cascade,
    command_type text not null,
    success boolean not null,
    error_code text,
    error_message text,
    result_payload jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);

create index if not exists command_receipts_player_id_idx
    on public.command_receipts (player_id);

alter table public.command_receipts enable row level security;

create policy "command_receipts_read_own"
    on public.command_receipts
    for select
    to authenticated
    using (player_id = auth.uid());

-- ---------------------------------------------------------------------------
-- Domain events
-- ---------------------------------------------------------------------------
create table if not exists public.domain_events (
    id uuid primary key default gen_random_uuid(),
    event_type text not null,
    player_id uuid references public.players(id) on delete set null,
    request_id text,
    payload jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);

create index if not exists domain_events_player_id_idx
    on public.domain_events (player_id);
create index if not exists domain_events_event_type_idx
    on public.domain_events (event_type);
create index if not exists domain_events_created_at_idx
    on public.domain_events (created_at desc);

alter table public.domain_events enable row level security;

create policy "domain_events_read_own"
    on public.domain_events
    for select
    to authenticated
    using (player_id = auth.uid());

-- ---------------------------------------------------------------------------
-- TRAVEL command (validated server mutation)
-- ---------------------------------------------------------------------------
create or replace function public.naad_travel(
    p_request_id text,
    p_to_location_id uuid
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player_id uuid := auth.uid();
    v_from_id uuid;
    v_to_exists boolean;
    v_receipt public.command_receipts%rowtype;
    v_result jsonb;
begin
    if v_player_id is null then
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'UNAUTHENTICATED',
            'errorMessage', 'Not authenticated'
        );
    end if;

    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'INVALID_REQUEST',
            'errorMessage', 'request_id is required'
        );
    end if;

    -- Idempotency: return prior result
    select * into v_receipt
    from public.command_receipts
    where request_id = p_request_id;

    if found then
        return jsonb_build_object(
            'requestId', v_receipt.request_id,
            'success', v_receipt.success,
            'errorCode', v_receipt.error_code,
            'errorMessage', v_receipt.error_message,
            'payload', v_receipt.result_payload
        );
    end if;

    -- Destination must exist
    select exists(select 1 from public.locations where id = p_to_location_id)
    into v_to_exists;

    if not v_to_exists then
        v_result := jsonb_build_object(
            'fromLocationId', null,
            'toLocationId', p_to_location_id
        );
        insert into public.command_receipts (
            request_id, player_id, command_type, success, error_code, error_message, result_payload
        ) values (
            p_request_id, v_player_id, 'TRAVEL', false, 'LOCATION_NOT_FOUND',
            'Destination location does not exist', v_result
        );
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'LOCATION_NOT_FOUND',
            'errorMessage', 'Destination location does not exist',
            'payload', v_result
        );
    end if;

    -- Current location (default Apartment if missing)
    select location_id into v_from_id
    from public.player_locations
    where player_id = v_player_id;

    if v_from_id is null then
        v_from_id := 'a1111111-1111-1111-1111-111111111101';
        insert into public.player_locations (player_id, location_id)
        values (v_player_id, v_from_id)
        on conflict (player_id) do nothing;
    end if;

    if v_from_id = p_to_location_id then
        v_result := jsonb_build_object(
            'fromLocationId', v_from_id,
            'toLocationId', p_to_location_id
        );
        insert into public.command_receipts (
            request_id, player_id, command_type, success, error_code, error_message, result_payload
        ) values (
            p_request_id, v_player_id, 'TRAVEL', false, 'ALREADY_THERE',
            'Player is already at this location', v_result
        );
        return jsonb_build_object(
            'requestId', p_request_id,
            'success', false,
            'errorCode', 'ALREADY_THERE',
            'errorMessage', 'Player is already at this location',
            'payload', v_result
        );
    end if;

    -- Apply mutation
    insert into public.player_locations (player_id, location_id, updated_at)
    values (v_player_id, p_to_location_id, now())
    on conflict (player_id) do update
        set location_id = excluded.location_id,
            updated_at = now();

    v_result := jsonb_build_object(
        'fromLocationId', v_from_id,
        'toLocationId', p_to_location_id
    );

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values (
        'PLAYER_TRAVELED',
        v_player_id,
        p_request_id,
        v_result
    );

    insert into public.command_receipts (
        request_id, player_id, command_type, success, result_payload
    ) values (
        p_request_id, v_player_id, 'TRAVEL', true, v_result
    );

    return jsonb_build_object(
        'requestId', p_request_id,
        'success', true,
        'errorCode', null,
        'errorMessage', null,
        'payload', v_result
    );
end;
$$;

-- Authenticated clients may execute the travel command
revoke all on function public.naad_travel(text, uuid) from public;
grant execute on function public.naad_travel(text, uuid) to authenticated;
