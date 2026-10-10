# Worker (optional)

A background-service host separate from the API. Delete `workers/`, its entry in
`ProjectName.slnx`, `Dockerfile.worker`, the `worker` service in
`docker-compose.yml`, the worker build step in `.github/workflows/ci.yml` and
`tests/ProjectName.UnitTests/Workers/` to remove it.
