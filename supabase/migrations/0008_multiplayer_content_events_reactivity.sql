-- GATES 15–20
-- 15 Multiplayer rooms (presence ephemeral; progression stays in world model)
-- 16 (process only — no schema)
-- 17 Content expansion (locations + careers)
-- 18 Social graph opportunities
-- 19 World events engine
-- 20 World reactivity hooks

-- ===========================================================================
-- GATE 15 — Game rooms + presence (NOT authoritative for money/inventory)
-- ===========================================================================
create table if not exists public.game_rooms (
    id uuid primary key default gen_random_uuid(),
    location_id uuid not null references public.locations(id),
    max_players integer not null default 8 check (max_players between 2 and 32),
    current_players integer not null default 0 check (current_players >= 0),
    event_id uuid,
    status text not null default 'OPEN' check (status in ('OPEN','FULL','CLOSED')),
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create index if not exists game_rooms_location_idx on public.game_rooms (location_id, status);

create table if not exists public.room_presence (
    room_id uuid not null references public.game_rooms(id) on delete cascade,
    player_id uuid not null references public.players(id) on delete cascade,
    display_name text not null default 'Player',
    pos_x real not null default 0,
    pos_y real not null default 0,
    pos_z real not null default 0,
    rot_y real not null default 0,
    animation text not null default 'idle',
    emote text,
    joined_at timestamptz not null default now(),
    last_seen_at timestamptz not null default now(),
    primary key (room_id, player_id)
);

create index if not exists room_presence_player_idx on public.room_presence (player_id);

alter table public.game_rooms enable row level security;
alter table public.room_presence enable row level security;

create policy "game_rooms_read_authenticated"
    on public.game_rooms for select to authenticated using (true);

create policy "room_presence_read_authenticated"
    on public.room_presence for select to authenticated using (true);

-- Join or create a room at a location
create or replace function public.naad_join_room(
    p_request_id text,
    p_location_id uuid
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_room public.game_rooms%rowtype;
    v_name text;
    v_receipt public.command_receipts%rowtype;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    if p_request_id is null or length(trim(p_request_id)) = 0 then
        return jsonb_build_object('success', false, 'errorCode', 'INVALID_REQUEST');
    end if;

    select * into v_receipt from public.command_receipts where request_id = p_request_id;
    if found then
        return jsonb_build_object('requestId', p_request_id, 'success', v_receipt.success, 'payload', v_receipt.result_payload);
    end if;

    if not exists (select 1 from public.locations where id = p_location_id) then
        return jsonb_build_object('success', false, 'errorCode', 'LOCATION_NOT_FOUND');
    end if;

    -- Leave any existing room first
    delete from public.room_presence where player_id = v_player;

    -- Find open room with space
    select * into v_room from public.game_rooms
    where location_id = p_location_id and status = 'OPEN' and current_players < max_players
    order by created_at asc
    limit 1
    for update;

    if not found then
        insert into public.game_rooms (location_id, max_players, current_players, status)
        values (p_location_id, 8, 0, 'OPEN')
        returning * into v_room;
    end if;

    select coalesce(display_name, 'Player') into v_name from public.players where id = v_player;

    insert into public.room_presence (room_id, player_id, display_name)
    values (v_room.id, v_player, v_name)
    on conflict (room_id, player_id) do update set last_seen_at = now();

    update public.game_rooms
    set current_players = (select count(*) from public.room_presence where room_id = v_room.id),
        status = case
            when (select count(*) from public.room_presence where room_id = v_room.id) >= max_players then 'FULL'
            else 'OPEN'
        end,
        updated_at = now()
    where id = v_room.id
    returning * into v_room;

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('ROOM_JOINED', v_player, p_request_id,
        jsonb_build_object('roomId', v_room.id, 'locationId', p_location_id, 'currentPlayers', v_room.current_players));

    insert into public.command_receipts (request_id, player_id, command_type, success, result_payload)
    values (p_request_id, v_player, 'JOIN_ROOM', true,
        jsonb_build_object('roomId', v_room.id, 'locationId', p_location_id, 'currentPlayers', v_room.current_players, 'maxPlayers', v_room.max_players));

    return jsonb_build_object(
        'requestId', p_request_id,
        'success', true,
        'payload', jsonb_build_object(
            'roomId', v_room.id,
            'locationId', p_location_id,
            'currentPlayers', v_room.current_players,
            'maxPlayers', v_room.max_players
        )
    );
end;
$$;

revoke all on function public.naad_join_room(text, uuid) from public;
grant execute on function public.naad_join_room(text, uuid) to authenticated;

create or replace function public.naad_leave_room(p_request_id text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_room_id uuid;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select room_id into v_room_id from public.room_presence where player_id = v_player;
    delete from public.room_presence where player_id = v_player;

    if v_room_id is not null then
        update public.game_rooms
        set current_players = (select count(*) from public.room_presence where room_id = v_room_id),
            status = case when (select count(*) from public.room_presence where room_id = v_room_id) = 0 then 'CLOSED' else 'OPEN' end,
            updated_at = now()
        where id = v_room_id;
    end if;

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('ROOM_LEFT', v_player, p_request_id, jsonb_build_object('roomId', v_room_id));

    return jsonb_build_object('requestId', p_request_id, 'success', true, 'payload', jsonb_build_object('roomId', v_room_id));
end;
$$;

revoke all on function public.naad_leave_room(text) from public;
grant execute on function public.naad_leave_room(text) to authenticated;

-- Ephemeral pose update (not progression)
create or replace function public.naad_update_presence(
    p_pos_x real, p_pos_y real, p_pos_z real, p_rot_y real,
    p_animation text default 'idle', p_emote text default null
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    update public.room_presence set
        pos_x = p_pos_x, pos_y = p_pos_y, pos_z = p_pos_z, rot_y = p_rot_y,
        animation = coalesce(nullif(p_animation, ''), 'idle'),
        emote = p_emote,
        last_seen_at = now()
    where player_id = v_player;

    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'NOT_IN_ROOM');
    end if;
    return jsonb_build_object('success', true);
end;
$$;

revoke all on function public.naad_update_presence(real, real, real, real, text, text) from public;
grant execute on function public.naad_update_presence(real, real, real, real, text, text) to authenticated;

-- ===========================================================================
-- GATE 17 — Content expansion
-- ===========================================================================
insert into public.locations (id, name, type, district_id) values
    ('a1111111-1111-1111-1111-111111111106', 'Restaurant', 'RESTAURANT', 'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111107', 'Lounge', 'LOUNGE', 'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111108', 'Hotel', 'HOTEL', 'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111109', 'ATM', 'OTHER', 'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-11111111110a', 'Gas Station', 'OTHER', 'd1111111-1111-1111-1111-111111111101')
on conflict (id) do nothing;

-- Careers / jobs catalog
create table if not exists public.career_defs (
    id text primary key,
    title text not null,
    description text not null default ''
);

insert into public.career_defs (id, title, description) values
    ('DEVELOPER', 'Developer', 'Ship code between nights out'),
    ('DJ', 'DJ', 'Control the night energy'),
    ('DRIVER', 'Driver', 'Move people across the district'),
    ('EVENT_PROMOTER', 'Event Promoter', 'Fill rooms and beaches'),
    ('RESTAURANT_WORKER', 'Restaurant Worker', 'Service and hustle')
on conflict (id) do nothing;

alter table public.career_defs enable row level security;
create policy "career_defs_read_authenticated"
    on public.career_defs for select to authenticated using (true);

create table if not exists public.player_careers (
    player_id uuid not null references public.players(id) on delete cascade,
    career_id text not null references public.career_defs(id),
    status text not null default 'ACTIVE' check (status in ('ACTIVE','PAUSED','COMPLETED')),
    started_at timestamptz not null default now(),
    primary key (player_id, career_id)
);

alter table public.player_careers enable row level security;
create policy "player_careers_read_own"
    on public.player_careers for select to authenticated using (player_id = auth.uid());

create or replace function public.naad_set_career(p_request_id text, p_career_id text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    if not exists (select 1 from public.career_defs where id = p_career_id) then
        return jsonb_build_object('success', false, 'errorCode', 'CAREER_NOT_FOUND');
    end if;

    insert into public.player_careers (player_id, career_id, status)
    values (v_player, p_career_id, 'ACTIVE')
    on conflict (player_id, career_id) do update set status = 'ACTIVE';

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('CAREER_SET', v_player, p_request_id, jsonb_build_object('careerId', p_career_id));

    return jsonb_build_object('requestId', p_request_id, 'success', true, 'payload', jsonb_build_object('careerId', p_career_id));
end;
$$;

revoke all on function public.naad_set_career(text, text) from public;
grant execute on function public.naad_set_career(text, text) to authenticated;

-- ===========================================================================
-- GATE 19 — World events
-- ===========================================================================
create table if not exists public.world_events (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    location_id uuid not null references public.locations(id),
    start_time timestamptz not null,
    end_time timestamptz not null,
    required_reputation integer not null default 0,
    crowd_profile text not null default 'MEDIUM'
        check (crowd_profile in ('LOW','MEDIUM','HIGH','PACKED')),
    status text not null default 'SCHEDULED'
        check (status in ('SCHEDULED','ACTIVE','ENDED','CANCELLED')),
    created_at timestamptz not null default now(),
    constraint world_events_time check (end_time > start_time)
);

create index if not exists world_events_location_idx on public.world_events (location_id, start_time);
create index if not exists world_events_status_idx on public.world_events (status);

alter table public.world_events enable row level security;
create policy "world_events_read_authenticated"
    on public.world_events for select to authenticated using (true);

-- FK from game_rooms.event_id
do $$
begin
    if not exists (
        select 1 from information_schema.table_constraints
        where constraint_name = 'game_rooms_event_id_fkey'
    ) then
        alter table public.game_rooms
            add constraint game_rooms_event_id_fkey
            foreign key (event_id) references public.world_events(id);
    end if;
end $$;

create table if not exists public.event_attendance (
    event_id uuid not null references public.world_events(id) on delete cascade,
    player_id uuid not null references public.players(id) on delete cascade,
    joined_at timestamptz not null default now(),
    primary key (event_id, player_id)
);

alter table public.event_attendance enable row level security;
create policy "event_attendance_read_own"
    on public.event_attendance for select to authenticated using (player_id = auth.uid());

-- Seed signature events
insert into public.world_events (id, name, location_id, start_time, end_time, required_reputation, crowd_profile, status)
values
(
    'f1111111-1111-1111-1111-111111111101',
    'Beach Party',
    'a1111111-1111-1111-1111-111111111105',
    now() + interval '1 hour',
    now() + interval '5 hours',
    0, 'HIGH', 'SCHEDULED'
),
(
    'f1111111-1111-1111-1111-111111111102',
    'Club Night',
    'a1111111-1111-1111-1111-111111111104',
    now() + interval '2 hours',
    now() + interval '6 hours',
    0, 'PACKED', 'SCHEDULED'
)
on conflict (id) do nothing;

create or replace function public.naad_list_events()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
begin
    if auth.uid() is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;
    return jsonb_build_object(
        'success', true,
        'events', coalesce((
            select jsonb_agg(to_jsonb(e.*) order by e.start_time)
            from public.world_events e
            where e.status in ('SCHEDULED','ACTIVE')
              and e.end_time > now()
        ), '[]'::jsonb)
    );
end;
$$;

revoke all on function public.naad_list_events() from public;
grant execute on function public.naad_list_events() to authenticated;

create or replace function public.naad_attend_event(p_request_id text, p_event_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_ev public.world_events%rowtype;
    v_count int;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_ev from public.world_events where id = p_event_id;
    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'EVENT_NOT_FOUND');
    end if;
    if v_ev.status = 'ENDED' or v_ev.status = 'CANCELLED' then
        return jsonb_build_object('success', false, 'errorCode', 'EVENT_CLOSED');
    end if;

    insert into public.event_attendance (event_id, player_id)
    values (p_event_id, v_player)
    on conflict do nothing;

    select count(*) into v_count from public.event_attendance where event_id = p_event_id;

    -- Move player to event location
    insert into public.player_locations (player_id, location_id, updated_at)
    values (v_player, v_ev.location_id, now())
    on conflict (player_id) do update set location_id = excluded.location_id, updated_at = now();

    update public.world_events set status = 'ACTIVE' where id = p_event_id and status = 'SCHEDULED';

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('EVENT_ATTENDED', v_player, p_request_id,
        jsonb_build_object('eventId', p_event_id, 'name', v_ev.name, 'attendance', v_count));

    -- GATE 20 reactivity trigger
    perform public.naad_react_to_event_attendance(p_event_id, v_count);

    return jsonb_build_object(
        'requestId', p_request_id,
        'success', true,
        'payload', jsonb_build_object(
            'eventId', p_event_id,
            'locationId', v_ev.location_id,
            'attendance', v_count
        )
    );
end;
$$;

revoke all on function public.naad_attend_event(text, uuid) from public;
grant execute on function public.naad_attend_event(text, uuid) to authenticated;

-- ===========================================================================
-- GATE 18 — Social graph opportunities
-- ===========================================================================
create table if not exists public.opportunities (
    id uuid primary key default gen_random_uuid(),
    player_id uuid not null references public.players(id) on delete cascade,
    source_npc_id uuid references public.npcs(id),
    opportunity_type text not null
        check (opportunity_type in ('INTRO','GIG','EVENT_ACCESS','JOB_OFFER','FAVOR')),
    title text not null,
    description text not null default '',
    status text not null default 'OPEN'
        check (status in ('OPEN','ACCEPTED','DECLINED','EXPIRED')),
    payload jsonb not null default '{}'::jsonb,
    created_at timestamptz not null default now()
);

create index if not exists opportunities_player_idx on public.opportunities (player_id, status);

alter table public.opportunities enable row level security;
create policy "opportunities_read_own"
    on public.opportunities for select to authenticated using (player_id = auth.uid());

-- When trust with Tunde is high enough, open a DJ intro opportunity
create or replace function public.naad_refresh_opportunities()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_tunde uuid := 'e1111111-1111-1111-1111-111111111101';
    v_trust int;
    v_id uuid;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select trust into v_trust from public.relationships
    where subject_id = v_player and target_id = v_tunde;

    if coalesce(v_trust, 0) >= 55 then
        if not exists (
            select 1 from public.opportunities
            where player_id = v_player and opportunity_type = 'INTRO'
              and status = 'OPEN'
              and payload->>'from' = 'Tunde'
        ) then
            insert into public.opportunities (
                player_id, source_npc_id, opportunity_type, title, description, payload
            ) values (
                v_player, v_tunde, 'INTRO',
                'Meet the DJ',
                'Tunde can introduce you to a DJ who needs a runner for Beach Party.',
                jsonb_build_object('from', 'Tunde', 'targetRole', 'DJ', 'eventHint', 'Beach Party')
            ) returning id into v_id;

            insert into public.domain_events (event_type, player_id, payload)
            values ('OPPORTUNITY_OPENED', v_player, jsonb_build_object('opportunityId', v_id, 'type', 'INTRO'));
        end if;
    end if;

    return jsonb_build_object(
        'success', true,
        'opportunities', coalesce((
            select jsonb_agg(to_jsonb(o.*) order by o.created_at desc)
            from public.opportunities o
            where o.player_id = v_player and o.status = 'OPEN'
        ), '[]'::jsonb)
    );
end;
$$;

revoke all on function public.naad_refresh_opportunities() from public;
grant execute on function public.naad_refresh_opportunities() to authenticated;

create or replace function public.naad_accept_opportunity(p_request_id text, p_opportunity_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_player uuid := auth.uid();
    v_opp public.opportunities%rowtype;
begin
    if v_player is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select * into v_opp from public.opportunities
    where id = p_opportunity_id and player_id = v_player and status = 'OPEN';
    if not found then
        return jsonb_build_object('success', false, 'errorCode', 'OPPORTUNITY_NOT_FOUND');
    end if;

    update public.opportunities set status = 'ACCEPTED' where id = p_opportunity_id;

    -- Small reputation-adjacent relationship bump with source NPC
    if v_opp.source_npc_id is not null then
        perform public.naad_apply_relationship_delta(
            p_request_id || ':rel', v_opp.source_npc_id, 2, 3, 0, 1, 0, 'Accepted opportunity'
        );
    end if;

    insert into public.domain_events (event_type, player_id, request_id, payload)
    values ('OPPORTUNITY_ACCEPTED', v_player, p_request_id,
        jsonb_build_object('opportunityId', p_opportunity_id, 'type', v_opp.opportunity_type));

    return jsonb_build_object('requestId', p_request_id, 'success', true,
        'payload', jsonb_build_object('opportunityId', p_opportunity_id, 'title', v_opp.title));
end;
$$;

revoke all on function public.naad_accept_opportunity(text, uuid) from public;
grant execute on function public.naad_accept_opportunity(text, uuid) to authenticated;

-- ===========================================================================
-- GATE 20 — World reactivity
-- ===========================================================================
create or replace function public.naad_react_to_event_attendance(p_event_id uuid, p_attendance integer)
returns void
language plpgsql
security definer
set search_path = public
as $$
declare
    v_ev public.world_events%rowtype;
    v_traffic int;
    v_night int;
begin
    select * into v_ev from public.world_events where id = p_event_id;
    if not found then return; end if;

    -- More attendance → higher nightlife + traffic pressure
    v_night := least(100, 50 + p_attendance * 5);
    v_traffic := least(100, 40 + p_attendance * 4);

    update public.world_state
    set nightlife_level = v_night,
        traffic_level = v_traffic
    where id = 1;

    insert into public.domain_events (event_type, player_id, payload)
    values (
        'WORLD_REACTIVITY',
        null,
        jsonb_build_object(
            'cause', 'EVENT_ATTENDANCE',
            'eventId', p_event_id,
            'eventName', v_ev.name,
            'attendance', p_attendance,
            'nightlifeLevel', v_night,
            'trafficLevel', v_traffic
        )
    );
end;
$$;

-- List room presence for a location (for local multiplayer view)
create or replace function public.naad_list_room_presence(p_location_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_room_id uuid;
begin
    if auth.uid() is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    select id into v_room_id from public.game_rooms
    where location_id = p_location_id and status in ('OPEN','FULL')
    order by updated_at desc limit 1;

    if v_room_id is null then
        return jsonb_build_object('success', true, 'roomId', null, 'members', '[]'::jsonb);
    end if;

    return jsonb_build_object(
        'success', true,
        'roomId', v_room_id,
        'members', coalesce((
            select jsonb_agg(jsonb_build_object(
                'playerId', player_id,
                'displayName', display_name,
                'pos', jsonb_build_object('x', pos_x, 'y', pos_y, 'z', pos_z),
                'rotY', rot_y,
                'animation', animation,
                'emote', emote
            ))
            from public.room_presence where room_id = v_room_id
        ), '[]'::jsonb)
    );
end;
$$;

revoke all on function public.naad_list_room_presence(uuid) from public;
grant execute on function public.naad_list_room_presence(uuid) to authenticated;
