import React from "react";
import { Stack, Box, Typography } from '@mui/material';
import { alpha } from '@mui/material/styles';

export default function SectionHeader({ icon, title, action }) {
  return (
    <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mb: 2.5 }}>
      <Stack direction="row" spacing={1.2} alignItems="center">
        <Box
          sx={{
            width: 36, height: 36, borderRadius: 2.5, display: 'grid', placeItems: 'center',
            bgcolor: (t) => alpha(t.palette.primary.main, 0.1), color: 'primary.main',
          }}
        >
          {icon}
        </Box>
        <Typography variant="h6">{title}</Typography>
      </Stack>
      {action}
    </Stack>
  );
}