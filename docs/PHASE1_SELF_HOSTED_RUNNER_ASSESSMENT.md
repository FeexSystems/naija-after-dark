# Phase 1 Self-Hosted Unity Runner Assessment

- Record ID: P1-002
- Date: 2026-10-22
- Repository: FeexSystems/naija-after-dark
- Related PR: https://github.com/FeexSystems/naija-after-dark/pull/11
- Scope: Investigate self-hosted execution of the exact Unity Editor 6000.7.0b4.
- Status: Assessment only. No runner registered, no secrets changed, no workflow routed to a self-hosted runner, no merge or deployment.

## Decision summary

**Conditional GO for a controlled proof-of-capability on an isolated, disposable runner; NO-GO for attaching a persistent self-hosted runner to this public repository's general pull-request workflow.**

The current GameCI run failed before Unity launched because the Docker image tag `unityci/editor:ubuntu-6000.7.0b4-linux-il2cpp-3` was unavailable. A self-hosted GitHub Actions runner does not by itself fix this: the existing `gameci/unity-builder@v4` action still requests its container image. To test the exact editor on a self-hosted host, the job would need either:
1. a verified custom GameCI-compatible image containing the exact editor and required modules; or
2. a separate, carefully reviewed direct-host workflow that invokes the locally installed Unity Editor in batch mode instead of invoking the unavailable GameCI container image.

Do not change the editor target or package versions as a workaround for this image lookup failure.

## Public-repository runner security

GitHub's security guidance says self-hosted runners do not have the clean, ephemeral isolation guarantees of GitHub-hosted runners and can be persistently compromised by untrusted workflow code. GitHub advises that self-hosted runners should almost never be used for public repositories because any user may open a pull request.

The existing workflow includes a `pull_request` trigger. Therefore, **do not simply change `runs-on: ubuntu-latest` to a self-hosted label** in this workflow.

Minimum safe boundary for a proof run:
- Use a dedicated, single-purpose VM or machine; do not reuse a developer workstation or a host containing production credentials.
- Prefer a fresh disposable VM per run, destroyed after the job. If true ephemerality is unavailable, treat this as a higher-risk exception and do not run untrusted PR code on it.
- Register the runner in a restricted runner group with access only to a dedicated validation repository or narrowly scoped trusted workflow.
- Do not execute pull-request code from forks, or any workflow revision that an untrusted contributor can alter, on the runner.
- Start with a trusted, reviewed commit dispatched manually or pushed to a protected trusted branch. Keep PR checks on GitHub-hosted runners until a separate security design is reviewed.
- Use a low-privilege runner identity, no persistent GitHub write token, no cloud metadata credentials, no production Supabase access, and no shared SSH keys.
- Restrict outbound network access to what checkout, Unity licensing, package registries, and required artifact upload need. Do not expose internal networks.
- Keep the runner's work directory and Unity caches disposable; never persist secrets or activation files in artifacts, logs, or caches.
- Ensure job cleanup and runner deregistration are tested. A failed cleanup must be treated as a security incident, not ignored.

## Unity installation and licensing

The runner must have the **exact Editor version 6000.7.0b4** installed and launchable, with the Linux build support required by the project. Record the full Editor version output, installation source, installed modules, OS image digest, and test commit SHA.

The repository workflow currently accepts either `UNITY_LICENSE` or the pair `UNITY_EMAIL` / `UNITY_PASSWORD`; this is configuration detection, not proof that the selected license can legally or technically activate the editor on this runner.

Unity's current documentation says Unity Personal is activated and returned through Unity Hub; manual activation is not supported for Personal. Paid/organization license activation methods differ. Before configuring automation:
1. Confirm the actual license tier and that its terms permit this CI/build use.
2. Confirm how the exact runner machine will activate, retain, and return the license; do not assume a license file generated on another machine will work.
3. Store only required activation material in the appropriate secret store; never commit it or print it.
4. Use a dedicated licensed seat or an approved Unity license server if required by the organization's entitlement. Do not bypass seat limits or license checks.
5. Test activation and deactivation/return behavior before adding parallel jobs. A shared or concurrent license may be exhausted or locked by a failed job.
6. Do not place license credentials in a public fork PR context or make them available to arbitrary workflow revisions.

## Proposed proof-of-capability gates

1. **Infrastructure gate:** owner confirms runner host, OS, isolation/ephemerality, runner group, and trusted trigger boundary.
2. **License gate:** license tier and entitlement are confirmed; exact machine activation succeeds without leaking credentials.
3. **Editor gate:** launch Unity 6000.7.0b4 in batch mode and capture the editor version and clean startup log.
4. **Package gate:** resolve packages from a clean checkout and capture Package Manager diagnostics. This is the first gate that can test the existing dependency blockers.
5. **Compile gate:** run the project's verification method and retain its report/log as artifacts, redacting sensitive paths or tokens.
6. **Repeatability gate:** run the same trusted commit twice on fresh environments and compare results.
7. **Security review:** verify the runner was destroyed/reset, license state handled correctly, no production services were contacted, and no secrets appeared in logs/artifacts.

No later gameplay, animation, live Supabase, or release gate should be called passed until its own test actually runs and produces evidence.

## Current unknowns / blockers

- No self-hosted runner is registered or available to this investigation.
- Runner OS, cloud/provider, VM isolation method, network boundary, and owner are not established.
- Unity license tier and current activation-seat availability are unknown.
- No direct-host Unity workflow has been implemented or tested.
- Exact Unity 6000.7.0b4 package resolution and project compilation remain untested.
- MCP/tooling dependencies remain untouched pending separate usage and dependency evidence.

## References

- GitHub Actions secure use / self-hosted runner hardening: https://docs.github.com/en/actions/reference/security/secure-use#hardening-for-self-hosted-runners
- Unity license activation methods: https://docs.unity.com/en-us/engine/6000.0/manual/get-started/install-and-upgrade/licenses-and-activation/license-activation-methods
- GameCI activation guidance: https://game.ci/docs/github/activation/
- Failed exact beta-image run: https://github.com/FeexSystems/naija-after-dark/actions/runs/39805703655
