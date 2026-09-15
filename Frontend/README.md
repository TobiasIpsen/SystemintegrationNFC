# Campus Access SPA

React + MUI app with three tabs, backed by a local Postgres + MinIO stack
(no cloud, no accounts needed to try it out):

- **User Panel** — create a new student (name, webcam photo, card ID, class, joined events).
- **Admin Panel** — create/edit/delete events, and create/edit/delete users.
- **Event Access** — pick an event, scan a card ID, and see the result:
  - 🟢 Green = Allowed
  - 🟠 Orange = User not created / hasn't joined this event
  - 🔴 Red = Already accessed this event
  - History list below shows everyone scanned in for that event (no duplicates).

## Architecture

```
[Browser: React app] --webcam capture--> IndexedDB (local cache, works offline)
        │
        │  on save, uploads photo + saves student/event via REST
        ▼
[Express API  (server/)]
        │                    │
        ▼                    ▼
   [Postgres]            [MinIO]  (S3-compatible local object storage)
```

- **Postgres** holds students, events, event membership, and the access log.
- **MinIO** holds the actual photo files and hands back a URL — same API
  shape as real S3, so pointing this at real AWS/GCS later is a config
  change in `server/storage.js`, not a rewrite.
- Photos are captured and cached in the browser (IndexedDB) the instant you
  take them, so capture works even with no connection. The upload to MinIO
  happens when you save the student, and `src/sync.js` retries automatically
  if you were offline when you saved.

## Run it

You need three things running at once:

**1. Postgres + MinIO**
```bash
docker compose up
```
This also creates the `student-photos` bucket automatically. MinIO's web
console is at http://localhost:9001 (login: `minioadmin` / `minioadmin`) if
you want to see uploaded photos land there.

**2. The API server**
```bash
cd server
npm install
cp .env.example .env   # defaults already match docker-compose, edit if needed
npm run dev
```

**3. The React app**
```bash
npm install
npm run dev
```
Then open the URL Vite prints (usually http://localhost:5173).

**Webcam access requires `https://` or `localhost`** — browsers block camera
access on plain `http://` for any other host. `npm run dev` serves on
localhost, so this works out of the box; if you deploy this for real you'll
need TLS.

## Where things live

| File | Purpose |
|---|---|
| `docker-compose.yml` | Local Postgres + MinIO |
| `server/schema.sql` | Database tables (students, events, student_events, access_log) |
| `server/index.js` | REST API the frontend talks to |
| `server/storage.js` | Uploads photos to MinIO (swap for real S3 later) |
| `src/api.js` | Frontend's client for the API above |
| `src/WebcamCapture.jsx` | Live camera capture UI |
| `src/photoStore.js` | IndexedDB cache so photos survive offline/refresh |
| `src/sync.js` | Retries photo uploads once you're back online |
