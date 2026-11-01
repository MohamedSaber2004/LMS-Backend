# OpenAPI / Postman sync

The Postman collection is a generated artifact. Nobody imports the Swagger document by hand any
more: every push to `master` regenerates the document and the collection from the commit that was
pushed, commits both, and publishes the collection into the Postman workspace through the Postman
API.

```
LMS.API (built in CI)  ->  openapi/v1.json  ->  postman/LMS.postman_collection.json  ->  Postman
```

## What runs

`.github/workflows/openapi-postman-sync.yml`

| Step | Script | Result |
| --- | --- | --- |
| Generate the document | `scripts/Update-OpenApi.ps1 -Launch` | Starts the API on the `http` profile, downloads `/swagger/v1/swagger.json`, writes the normalized `openapi/v1.json`, stops the API. |
| Build the collection | `scripts/Update-PostmanCollection.ps1 -BaseUrl $SITE_URL` | Rewrites `postman/LMS.postman_collection.json` from `openapi/v1.json`. |
| Coverage check | `scripts/Verify-PostmanCollection.ps1` | Fails when an operation in the document has no request in the collection. |
| Commit | `git add openapi postman` | Commits with `[skip ci]` so the run does not restart itself. |
| Publish | `scripts/Publish-PostmanCollection.ps1` | `PUT https://api.getpostman.com/collections/<uid>` replaces the collection in place. |
| Read back | `scripts/Verify-PostmanCollection.ps1 -FromPostman` | Reads the collection out of Postman and re-checks it against the document. |

The document is built from the source tree, not downloaded from the live site, so the collection
always describes the commit that was built and never lags a deploy behind. Nothing touches the
database during the run: the API only seeds when started with `--seed`, and booting it is enough to
serve the document.

Pull requests get the same generation and the same coverage check, but only report drift as a
warning and never publish: a fork has no access to the secrets, and the run that follows the merge
publishes the result anyway.

## One-time setup

1. **Create the collection in Postman once.** Import `postman/LMS.postman_collection.json`
   (Import file), then copy the collection UID from its URL:
   `https://web.postman.co/workspace/<...>/collection/<UID>`.
   Importing the generated file rather than the Swagger URL is what keeps the UID stable:
   `Publish-PostmanCollection.ps1` updates that collection in place afterwards.
2. **Add the repository secrets** in *Settings > Secrets and variables > Actions*:
   - `POSTMAN_API_KEY` - Postman > avatar > Integrations/API keys. Keep it out of every file,
     including this one; the scripts only read it from the environment.
   - `POSTMAN_COLLECTION_ID` - the UID from step 1.
3. **Optional repository variable** `SITE_URL` - the host baked into the collection's `baseUrl`
   variable. Default `https://lms-api.runasp.net`.

Then trigger *OpenAPI / Postman Sync > Run workflow* once to publish the current surface; later
pushes do it automatically.

## Using the collection

- `{{baseUrl}}` is the collection variable - point it at the environment you want to call
  (`https://lms-api.runasp.net`, `http://localhost:5029`, ...).
- `{{token}}` is what the collection bearer auth sends. Log in, copy the access token into
  `token`, and every request inherits it.
- `multipart/form-data` operations (register, the attachment uploads) come out as real Postman
  form fields; file fields (`Certificates`, `IntroVideo`, ...) arrive empty on purpose - pick the
  file before sending.
- Named response examples from the document (`Created`, `ValidationError`, ...) are saved as
  example responses on each request.

## Local runs

```powershell
.\scripts\Update-OpenApi.ps1 -Launch                  # or -BaseUrl https://lms-api.runasp.net
.\scripts\Update-PostmanCollection.ps1
.\scripts\Verify-PostmanCollection.ps1
$env:POSTMAN_API_KEY='...'; $env:POSTMAN_COLLECTION_ID='...'
.\scripts\Publish-PostmanCollection.ps1
```

PowerShell 7 (the runner) and Windows PowerShell 5.1 produce byte-identical artifacts: the
generated files are written by `scripts/StableJson.ps1` instead of `ConvertTo-Json`, so committing
from a developer machine never fights with a commit from CI.

## Rules

- Never hand-edit `openapi/v1.json` or `postman/LMS.postman_collection.json` - the next run
  overwrites them. Change the API, or the converter, instead.
- If the coverage step fails, the collection is behind the code: run the scripts, or just merge,
  because the push run regenerates both files.
- The Postman API key lives only in the repository secret. Rotating it is a single change in
  *Settings*, with nothing to commit.
