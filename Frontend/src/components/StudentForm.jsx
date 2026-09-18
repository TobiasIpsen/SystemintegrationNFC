import React from 'react';
import { Grid, TextField, Typography, Box, Chip } from '@mui/material';
import WebcamCapture, { CapturedPreview } from './WebcamCapture.jsx';

export default function StudentForm({ value, onChange, events }) {
  const toggleEvent = (id) => {
    const has = value.eventIds.includes(id);
    onChange({
      ...value,
      eventIds: has ? value.eventIds.filter((e) => e !== id) : [...value.eventIds, id],
    });
  };

  // Called with a fresh JPEG data URL from the webcam. We show it right away
  // and stash it in IndexedDB so it survives even if we're offline right
  // now. The actual upload to MinIO happens when the form is saved
  // (see resolveImageUrl) — that's the point where we know the student
  // record is really being created/updated.
  const handleImageCapture = async (dataUrl) => {
    onChange({ ...value, image: dataUrl });
  };

  return (
    <Grid container spacing={2.5}>
      <Grid item xs={12} sm="auto">
        {value.image ? (
          <CapturedPreview dataUrl={value.image} onRetake={() => onChange({ ...value, image: '' })} />
        ) : (
          <WebcamCapture onCapture={handleImageCapture} />
        )}
      </Grid>
      <Grid item xs={12} sm={6}>
        <TextField
          label="Name" fullWidth value={value.name}
          onChange={(e) => onChange({ ...value, name: e.target.value })}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <TextField
          label="Class" fullWidth value={value.className}
          onChange={(e) => onChange({ ...value, className: e.target.value })}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <TextField
          label="Card ID" fullWidth value={value.cardId}
          onChange={(e) => onChange({ ...value, cardId: e.target.value })}
        />
      </Grid>
      <Grid item xs={12}>
        <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
          JOINED EVENTS
        </Typography>
        <Box sx={{ mt: 1 }}>
          {events.length === 0 && (
            <Typography variant="body2" color="text.secondary">No events yet — create one in the Admin Panel.</Typography>
          )}
          {events.map((ev) => (
            <Chip
              key={ev.id}
              label={ev.name}
              onClick={() => toggleEvent(ev.id)}
              color={value.eventIds.includes(ev.id) ? 'secondary' : 'default'}
              variant={value.eventIds.includes(ev.id) ? 'filled' : 'outlined'}
              sx={{ mr: 1, mb: 1 }}
            />
          ))}
        </Box>
      </Grid>
    </Grid>
  );
}