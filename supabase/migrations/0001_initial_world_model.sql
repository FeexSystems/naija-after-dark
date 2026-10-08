-- GATE 1.2 / 1.3 — Initial World Model (authoritative)
-- The World Model is authoritative.
-- Unity is untrusted. Gemini cannot mutate state.
-- All money mutations go through validated server commands later.
-- No client-side UPDATE policies on wallets.

create extension if not exists "pgcrypto";

-- ---------------------------------------------------------------------------
-- players
-- ---------------------------------------------------------------------------
create table public.players (
    id uuid primary key references auth.users(id) on delete cascade,
    display_name text not null,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint players_display_name_not_empty check (char_length(trim(display_name)) > 0)
);

create index players_display_name_idx on public.players (display_name);

-- ---------------------------------------------------------------------------
-- wallets (authoritative money; never trust client balance)
-- ---------------------------------------------------------------------------
create table public.wallets (
    player_id uuid primary key references public.players(id) on delete cascade,
    currency text not null default 'NGN',
    balance bigint not null default 0,
    updated_at timestamptz not null default now(),
    constraint wallet_balance_nonnegative check (balance >= 0),
    constraint wallet_currency_ngn check (currency = 'NGN')
);

-- ---------------------------------------------------------------------------
-- locations
-- ---------------------------------------------------------------------------
create table public.locations (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    type text not null,
    district_id uuid,
    created_at timestamptz not null default now(),
    constraint locations_name_not_empty check (char_length(trim(name)) > 0),
    constraint locations_type_not_empty check (char_length(trim(type)) > 0)
);

create index locations_type_idx on public.locations (type);
create index locations_district_id_idx on public.locations (district_id);

-- ---------------------------------------------------------------------------
-- npcs
-- ---------------------------------------------------------------------------
create table public.npcs (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    age integer,
    archetype text,
    occupation text,
    current_location_id uuid references public.locations(id),
    mood integer not null default 50,
    reputation integer not null default 0,
    created_at timestamptz not null default now(),
    constraint npcs_name_not_empty check (char_length(trim(name)) > 0),
    constraint npcs_mood_range check (mood >= 0 and mood <= 100)
);

create index npcs_current_location_id_idx on public.npcs (current_location_id);
create index npcs_archetype_idx on public.npcs (archetype);

-- ---------------------------------------------------------------------------
-- relationships (subject → target)
-- ---------------------------------------------------------------------------
create table public.relationships (
    id uuid primary key default gen_random_uuid(),
    subject_id uuid not null,
    target_id uuid not null,
    trust integer not null default 50,
    respect integer not null default 50,
    affection integer not null default 0,
    loyalty integer not null default 0,
    conflict integer not null default 0,
    relationship_type text,
    updated_at timestamptz not null default now(),
    constraint relationships_subject_target_distinct check (subject_id <> target_id),
    constraint relationships_trust_range check (trust >= 0 and trust <= 100),
    constraint relationships_respect_range check (respect >= 0 and respect <= 100),
    constraint relationships_affection_range check (affection >= 0 and affection <= 100),
    constraint relationships_loyalty_range check (loyalty >= 0 and loyalty <= 100),
    constraint relationships_conflict_range check (conflict >= 0 and conflict <= 100),
    constraint relationships_subject_target_unique unique (subject_id, target_id)
);

create index relationships_subject_id_idx on public.relationships (subject_id);
create index relationships_target_id_idx on public.relationships (target_id);

-- ---------------------------------------------------------------------------
-- world_state (singleton row)
-- ---------------------------------------------------------------------------
create table public.world_state (
    id integer primary key default 1,
    game_time timestamptz not null default now(),
    weather text not null default 'CLEAR',
    traffic_level integer not null default 50,
    nightlife_level integer not null default 50,
    constraint world_state_singleton check (id = 1),
    constraint world_state_traffic_range check (traffic_level >= 0 and traffic_level <= 100),
    constraint world_state_nightlife_range check (nightlife_level >= 0 and nightlife_level <= 100)
);

insert into public.world_state (id) values (1);

-- ---------------------------------------------------------------------------
-- updated_at helper
-- ---------------------------------------------------------------------------
create or replace function public.set_updated_at()
returns trigger
language plpgsql
as $$
begin
    new.updated_at = now();
    return new;
end;
$$;

create trigger players_set_updated_at
    before update on public.players
    for each row execute function public.set_updated_at();

create trigger wallets_set_updated_at
    before update on public.wallets
    for each row execute function public.set_updated_at();

create trigger relationships_set_updated_at
    before update on public.relationships
    for each row execute function public.set_updated_at();

-- ---------------------------------------------------------------------------
-- GATE 1.3 — Row Level Security
-- Players and wallets: owner can SELECT only.
-- No client UPDATE/INSERT/DELETE on wallets (mutations via server commands).
-- ---------------------------------------------------------------------------
alter table public.players enable row level security;
alter table public.wallets enable row level security;
alter table public.locations enable row level security;
alter table public.npcs enable row level security;
alter table public.relationships enable row level security;
alter table public.world_state enable row level security;

-- players: read own row
create policy "players_read_own"
    on public.players
    for select
    to authenticated
    using (id = auth.uid());

-- wallets: read own row only (no client write)
create policy "wallet_read_own"
    on public.wallets
    for select
    to authenticated
    using (player_id = auth.uid());

-- locations: readable by authenticated (world is shared)
create policy "locations_read_authenticated"
    on public.locations
    for select
    to authenticated
    using (true);

-- npcs: readable by authenticated
create policy "npcs_read_authenticated"
    on public.npcs
    for select
    to authenticated
    using (true);

-- relationships: subject or target can read
create policy "relationships_read_participant"
    on public.relationships
    for select
    to authenticated
    using (subject_id = auth.uid() or target_id = auth.uid());

-- world_state: readable by authenticated
create policy "world_state_read_authenticated"
    on public.world_state
    for select
    to authenticated
    using (true);

-- Explicit: no policies granting INSERT/UPDATE/DELETE on wallets to authenticated.
-- Service role bypasses RLS for server-side command handlers.
