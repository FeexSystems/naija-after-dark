-- GATE 6 — World Time
-- Server owns simulation time. Device clock is never authoritative.
-- Periods: MORNING 06, DAY 09, TRANSITION 16, NIGHT 19, LATE_NIGHT 00, AFTER_HOURS 03

-- ---------------------------------------------------------------------------
-- Extend world_state with time_scale + last real-world advance marker
-- ---------------------------------------------------------------------------
alter table public.world_state
    add column if not exists time_scale numeric not null default 60
        check (time_scale > 0 and time_scale <= 10000),
    add column if not exists last_real_advance_at timestamptz not null default now();

comment on column public.world_state.time_scale is
    'Simulated minutes advanced per real minute. 60 = 1 game hour per real minute.';

-- Start the First Night near evening for demo default
update public.world_state
set game_time = date_trunc('day', now() at time zone 'UTC') + interval '19 hours',
    nightlife_level = 70,
    time_scale = 60,
    last_real_advance_at = now()
where id = 1;

-- ---------------------------------------------------------------------------
-- Derive period from minutes since midnight (server-side only)
-- ---------------------------------------------------------------------------
create or replace function public.naad_period_from_minutes(p_minutes integer)
returns text
language sql
immutable
as $$
    select case
        when p_minutes >= 360 and p_minutes < 540 then 'MORNING'       -- 06:00–08:59
        when p_minutes >= 540 and p_minutes < 960 then 'DAY'           -- 09:00–15:59
        when p_minutes >= 960 and p_minutes < 1140 then 'TRANSITION'   -- 16:00–18:59 sunset
        when p_minutes >= 1140 and p_minutes < 1440 then 'NIGHT'       -- 19:00–23:59
        when p_minutes >= 0 and p_minutes < 180 then 'LATE_NIGHT'      -- 00:00–02:59
        else 'AFTER_HOURS'                                            -- 03:00–05:59
    end;
$$;

create or replace function public.naad_nightlife_for_period(p_period text)
returns integer
language sql
immutable
as $$
    select case p_period
        when 'MORNING' then 15
        when 'DAY' then 25
        when 'TRANSITION' then 55
        when 'NIGHT' then 80
        when 'LATE_NIGHT' then 90
        when 'AFTER_HOURS' then 60
        else 50
    end;
$$;

-- ---------------------------------------------------------------------------
-- World clock read model (authoritative snapshot)
-- ---------------------------------------------------------------------------
create or replace function public.naad_get_world_clock()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_row public.world_state%rowtype;
    v_minutes integer;
    v_period text;
    v_date text;
begin
    if auth.uid() is null then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'UNAUTHENTICATED',
            'errorMessage', 'Not authenticated'
        );
    end if;

    select * into v_row from public.world_state where id = 1;
    if not found then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'WORLD_MISSING',
            'errorMessage', 'world_state row missing'
        );
    end if;

    v_minutes := (extract(hour from v_row.game_time) * 60
                + extract(minute from v_row.game_time))::integer;
    v_period := public.naad_period_from_minutes(v_minutes);
    v_date := to_char(v_row.game_time at time zone 'UTC', 'YYYY-MM-DD');

    return jsonb_build_object(
        'success', true,
        'gameDate', v_date,
        'gameTime', v_row.game_time,
        'minutesSinceMidnight', v_minutes,
        'period', v_period,
        'timeScale', v_row.time_scale,
        'weather', v_row.weather,
        'trafficLevel', v_row.traffic_level,
        'nightlifeLevel', v_row.nightlife_level
    );
end;
$$;

revoke all on function public.naad_get_world_clock() from public;
grant execute on function public.naad_get_world_clock() to authenticated;

-- ---------------------------------------------------------------------------
-- Advance simulation by real elapsed time * time_scale
-- ---------------------------------------------------------------------------
create or replace function public.naad_advance_world_time()
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_row public.world_state%rowtype;
    v_elapsed_minutes numeric;
    v_advance interval;
    v_old_period text;
    v_new_period text;
    v_old_minutes integer;
    v_new_minutes integer;
begin
    if auth.uid() is null then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'UNAUTHENTICATED'
        );
    end if;

    select * into v_row from public.world_state where id = 1 for update;

    v_elapsed_minutes := extract(epoch from (now() - v_row.last_real_advance_at)) / 60.0;
    if v_elapsed_minutes < 0 then
        v_elapsed_minutes := 0;
    end if;

    v_advance := make_interval(mins => (v_elapsed_minutes * v_row.time_scale)::integer);

    v_old_minutes := (extract(hour from v_row.game_time) * 60
                    + extract(minute from v_row.game_time))::integer;
    v_old_period := public.naad_period_from_minutes(v_old_minutes);

    update public.world_state
    set game_time = v_row.game_time + v_advance,
        last_real_advance_at = now(),
        nightlife_level = public.naad_nightlife_for_period(
            public.naad_period_from_minutes(
                (extract(hour from (v_row.game_time + v_advance)) * 60
               + extract(minute from (v_row.game_time + v_advance)))::integer
            )
        )
    where id = 1
    returning * into v_row;

    v_new_minutes := (extract(hour from v_row.game_time) * 60
                    + extract(minute from v_row.game_time))::integer;
    v_new_period := public.naad_period_from_minutes(v_new_minutes);

    if v_old_period is distinct from v_new_period then
        insert into public.domain_events (event_type, player_id, request_id, payload)
        values (
            'WORLD_PERIOD_CHANGED',
            auth.uid(),
            null,
            jsonb_build_object(
                'fromPeriod', v_old_period,
                'toPeriod', v_new_period,
                'gameTime', v_row.game_time,
                'minutesSinceMidnight', v_new_minutes
            )
        );
    end if;

    return public.naad_get_world_clock();
end;
$$;

revoke all on function public.naad_advance_world_time() from public;
grant execute on function public.naad_advance_world_time() to authenticated;

-- ---------------------------------------------------------------------------
-- Demo / test: jump to a named period (server still owns the write)
-- ---------------------------------------------------------------------------
create or replace function public.naad_set_world_period(p_period text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
    v_minutes integer;
    v_base date;
    v_old text;
    v_row public.world_state%rowtype;
begin
    if auth.uid() is null then
        return jsonb_build_object('success', false, 'errorCode', 'UNAUTHENTICATED');
    end if;

    v_minutes := case upper(p_period)
        when 'MORNING' then 6 * 60 + 30
        when 'DAY' then 12 * 60
        when 'TRANSITION' then 17 * 60 + 30   -- sunset
        when 'SUNSET' then 17 * 60 + 30
        when 'NIGHT' then 21 * 60
        when 'LATE_NIGHT' then 1 * 60
        when 'AFTER_HOURS' then 4 * 60
        else null
    end;

    if v_minutes is null then
        return jsonb_build_object(
            'success', false,
            'errorCode', 'INVALID_PERIOD',
            'errorMessage', 'Use MORNING|DAY|TRANSITION|NIGHT|LATE_NIGHT|AFTER_HOURS'
        );
    end if;

    select * into v_row from public.world_state where id = 1 for update;
    v_old := public.naad_period_from_minutes(
        (extract(hour from v_row.game_time) * 60 + extract(minute from v_row.game_time))::integer
    );
    v_base := (v_row.game_time at time zone 'UTC')::date;

    -- LATE_NIGHT / AFTER_HOURS are after midnight — keep same calendar day anchor
    update public.world_state
    set game_time = (v_base + make_interval(mins => v_minutes)) at time zone 'UTC',
        nightlife_level = public.naad_nightlife_for_period(upper(p_period)),
        last_real_advance_at = now()
    where id = 1;

    insert into public.domain_events (event_type, player_id, payload)
    values (
        'WORLD_PERIOD_CHANGED',
        auth.uid(),
        jsonb_build_object(
            'fromPeriod', v_old,
            'toPeriod', upper(p_period),
            'source', 'naad_set_world_period'
        )
    );

    return public.naad_get_world_clock();
end;
$$;

revoke all on function public.naad_set_world_period(text) from public;
grant execute on function public.naad_set_world_period(text) to authenticated;
