import React from 'react';
import { Stack, Typography } from '@mui/material';

export default function EmptyState({ icon, text }) {
  return (
    <Stack alignItems="center" spacing={1} sx={{ py: 5, color: 'text.secondary' }}>
      {icon}
      <Typography variant="body2">{text}</Typography>
    </Stack>
  );
}
