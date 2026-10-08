-- GATE 7 — One Night District (Lagos Metro → Night District → Locations)
-- GATE 8 — Five starter NPCs (Tunde AI-capable; others deterministic)

-- ---------------------------------------------------------------------------
-- Districts
-- ---------------------------------------------------------------------------
create table if not exists public.districts (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    metro text not null default 'LAGOS',
    created_at timestamptz not null default now(),
    constraint districts_name_not_empty check (char_length(trim(name)) > 0)
);

alter table public.districts enable row level security;

create policy "districts_read_authenticated"
    on public.districts for select to authenticated using (true);

-- Night District
insert into public.districts (id, name, metro)
values ('d1111111-1111-1111-1111-111111111101', 'Night District', 'LAGOS')
on conflict (id) do nothing;

-- ---------------------------------------------------------------------------
-- Locations (Apartment + Suya already seeded in 0003)
-- ---------------------------------------------------------------------------
insert into public.locations (id, name, type, district_id)
values
    ('a1111111-1111-1111-1111-111111111101', 'Apartment', 'APARTMENT',
        'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111102', 'Suya Spot', 'SUYA_SPOT',
        'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111103', 'Street', 'STREET',
        'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111104', 'Nightclub', 'NIGHTCLUB',
        'd1111111-1111-1111-1111-111111111101'),
    ('a1111111-1111-1111-1111-111111111105', 'Beach', 'BEACH',
        'd1111111-1111-1111-1111-111111111101')
on conflict (id) do update
    set district_id = excluded.district_id,
        name = excluded.name,
        type = excluded.type;

-- ---------------------------------------------------------------------------
-- Businesses
-- ---------------------------------------------------------------------------
create table if not exists public.businesses (
    id uuid primary key default gen_random_uuid(),
    name text not null,
    location_id uuid not null references public.locations(id),
    owner_npc_id uuid,
    created_at timestamptz not null default now()
);

alter table public.businesses enable row level security;
create policy "businesses_read_authenticated"
    on public.businesses for select to authenticated using (true);

-- ---------------------------------------------------------------------------
-- Items (catalog)
-- ---------------------------------------------------------------------------
create table if not exists public.items (
    id uuid primary key default gen_random_uuid(),
    sku text not null unique,
    name text not null,
    category text not null,
    base_price_ngn bigint not null check (base_price_ngn >= 0),
    constraint items_category_check check (category in ('FOOD','DRINK','TICKET','CLOTHING','MISC'))
);

alter table public.items enable row level security;
create policy "items_read_authenticated"
    on public.items for select to authenticated using (true);

insert into public.items (id, sku, name, category, base_price_ngn) values
    ('11111111-1111-1111-1111-111111111101', 'SUYA_PLATE', 'Suya Plate', 'FOOD', 5000),
    ('11111111-1111-1111-1111-111111111102', 'CLUB_TICKET', 'Nightclub Ticket', 'TICKET', 20000),
    ('11111111-1111-1111-1111-111111111103', 'SOFT_DRINK', 'Soft Drink', 'DRINK', 1500)
on conflict (sku) do nothing;

-- ---------------------------------------------------------------------------
-- NPCs (fixed ids for content + AI)
-- ---------------------------------------------------------------------------
-- Extend npcs with personality fields used by AI (optional columns)
alter table public.npcs
    add column if not exists personality text,
    add column if not exists goal text,
    add column if not exists dialogue_mode text not null default 'DETERMINISTIC'
        check (dialogue_mode in ('DETERMINISTIC', 'AI'));

insert into public.npcs (
    id, name, age, archetype, occupation, current_location_id,
    mood, reputation, personality, goal, dialogue_mode
) values
(
    'e1111111-1111-1111-1111-111111111101',
    'Tunde', 28, 'PROMOTER', 'Event Promoter',
    'a1111111-1111-1111-1111-111111111104',
    65, 40,
    'Street-smart, warm, hustling, loyal once trust is earned. Speaks natural Nigerian English.',
    'Fill the beach and club with the right crowd tonight.',
    'AI'
),
(
    'e1111111-1111-1111-1111-111111111102',
    'Mama Seyi', 52, 'VENDOR', 'Suya Vendor',
    'a1111111-1111-1111-1111-111111111102',
    55, 60,
    'No-nonsense, motherly, proud of her suya.',
    'Sell out the grill before midnight.',
    'DETERMINISTIC'
),
(
    'e1111111-1111-1111-1111-111111111103',
    'Amaka', 26, 'CREATIVE', 'Creative',
    'a1111111-1111-1111-1111-111111111105',
    60, 25,
    'Observant, stylish, dry humor.',
    'Shoot content at the beach event.',
    'DETERMINISTIC'
),
(
    'e1111111-1111-1111-1111-111111111104',
    'Emeka', 30, 'DEVELOPER', 'Developer',
    'a1111111-1111-1111-1111-111111111101',
    50, 15,
    'Quiet, precise, slightly awkward socially.',
    'Ship a side project between nights out.',
    'DETERMINISTIC'
),
(
    'e1111111-1111-1111-1111-111111111105',
    'Dami', 27, 'SOCIALITE', 'Socialite',
    'a1111111-1111-1111-1111-111111111104',
    70, 55,
    'Charismatic, networked, always knows the next stop.',
    'Be seen at the right tables.',
    'DETERMINISTIC'
)
on conflict (id) do update set
    name = excluded.name,
    archetype = excluded.archetype,
    occupation = excluded.occupation,
    current_location_id = excluded.current_location_id,
    personality = excluded.personality,
    goal = excluded.goal,
    dialogue_mode = excluded.dialogue_mode;

-- Businesses after NPCs exist
insert into public.businesses (id, name, location_id, owner_npc_id) values
    ('b1111111-1111-1111-1111-111111111101', 'Mama Seyi Suya',
        'a1111111-1111-1111-1111-111111111102',
        'e1111111-1111-1111-1111-111111111102'),
    ('b1111111-1111-1111-1111-111111111102', 'Night District Club',
        'a1111111-1111-1111-1111-111111111104', null),
    ('b1111111-1111-1111-1111-111111111103', 'Beach Bar',
        'a1111111-1111-1111-1111-111111111105', null)
on conflict (id) do nothing;

-- FK owner_npc if not already
do $$
begin
    if not exists (
        select 1 from information_schema.table_constraints
        where constraint_name = 'businesses_owner_npc_id_fkey'
    ) then
        alter table public.businesses
            add constraint businesses_owner_npc_id_fkey
            foreign key (owner_npc_id) references public.npcs(id);
    end if;
end $$;
