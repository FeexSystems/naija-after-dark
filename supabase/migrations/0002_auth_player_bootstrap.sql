-- GATE 4 — Auth bootstrap
-- When a Supabase Auth user is created, automatically create
-- public.players + public.wallets. Client never inserts money rows.
-- Unity uses anon key only; service-role stays server-side.

-- Allow authenticated users to insert their own player row (id must = auth.uid()).
-- Prefer the trigger path; this policy is a safe fallback for explicit client ensure.
create policy "players_insert_own"
    on public.players
    for insert
    to authenticated
    with check (id = auth.uid());

-- Allow authenticated users to update their own display_name only (row-level).
create policy "players_update_own"
    on public.players
    for update
    to authenticated
    using (id = auth.uid())
    with check (id = auth.uid());

-- Auto-provision player + wallet on auth.users insert
create or replace function public.handle_new_user()
returns trigger
language plpgsql
security definer
set search_path = public
as $$
declare
    chosen_name text;
begin
    chosen_name := coalesce(
        nullif(trim(new.raw_user_meta_data->>'display_name'), ''),
        nullif(trim(split_part(new.email, '@', 1)), ''),
        'Player'
    );

    insert into public.players (id, display_name)
    values (new.id, chosen_name)
    on conflict (id) do nothing;

    insert into public.wallets (player_id, currency, balance)
    values (new.id, 'NGN', 100000)
    on conflict (player_id) do nothing;

    return new;
end;
$$;

drop trigger if exists on_auth_user_created on auth.users;

create trigger on_auth_user_created
    after insert on auth.users
    for each row
    execute function public.handle_new_user();

-- Backfill: ensure existing auth users (e.g. Gate1 tester) have player + wallet
insert into public.players (id, display_name)
select u.id, coalesce(nullif(trim(split_part(u.email, '@', 1)), ''), 'Player')
from auth.users u
where not exists (select 1 from public.players p where p.id = u.id)
on conflict (id) do nothing;

insert into public.wallets (player_id, currency, balance)
select p.id, 'NGN', 100000
from public.players p
where not exists (select 1 from public.wallets w where w.player_id = p.id)
on conflict (player_id) do nothing;
