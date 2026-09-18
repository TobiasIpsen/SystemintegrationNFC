import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, Grid, TextField, MenuItem, Button, Avatar, Chip, Table, TableHead, TableRow, TableCell, TableBody, Stack } from '@mui/material';
import { alpha } from '@mui/material/styles';
import NfcIcon from '@mui/icons-material/Nfc';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import CancelIcon from '@mui/icons-material/Cancel';
import HistoryOutlinedIcon from '@mui/icons-material/HistoryOutlined';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import SectionHeader from './SectionHeader.jsx';
import EmptyState from './EmptyState.jsx';
import * as api from '../api.js';

const STATUS = {
  allowed: { color: 'success.main', hex: '#1E9E5A', label: 'Allowed', Icon: CheckCircleIcon },
  notJoined: { color: 'warning.main', hex: '#E08A1E', label: 'Not Created / Not Joined This Event', Icon: WarningAmberIcon },
  already: { color: 'error.main', hex: '#D64550', label: 'Already Accessed This Event', Icon: CancelIcon },
};

export default function EventAccess({ students, events }) {
  const [eventId, setEventId] = useState(events[0]?.id || '');
  const [cardId, setCardId] = useState('');
  const [scan, setScan] = useState(null);
  const [history, setHistory] = useState([]);

  useEffect(() => {
    if (!eventId) { setHistory([]); return; }
    api.fetchAccessLog(eventId).then(setHistory).catch(() => setHistory([]));
  }, [eventId]);

  const handleScan = async () => {
    const student = students.find((s) => s.cardId === cardId.trim());

    if (!student || !student.eventIds.includes(eventId)) {
      setScan({ student: student || null, status: 'notJoined' });
      setCardId('');
      return;
    }

    if (history.includes(student.id)) {
      setScan({ student, status: 'already' });
      setCardId('');
      return;
    }

    try {
      await api.logAccess(eventId, student.id);
      setHistory((prev) => [...prev, student.id]);
      setScan({ student, status: 'allowed' });
    } catch {
      // Someone else scanned this student in the split second before us —
      // treat it the same as "already accessed".
      setHistory((prev) => [...prev, student.id]);
      setScan({ student, status: 'already' });
    }
    setCardId('');
  };

  const current = scan ? STATUS[scan.status] : null;

  return (
    <Box
      sx={{
        minHeight: '78vh',
        borderRadius: 4,
        p: { xs: 2.5, sm: 4 },
        transition: 'background 0.35s ease',
        background: current
          ? `linear-gradient(180deg, ${alpha(current.hex, 0.16)} 0%, ${alpha(current.hex, 0.04)} 45%)`
          : 'linear-gradient(180deg, #EEF0FB 0%, #F5F6FB 45%)',
      }}
    >
      <Paper sx={{ p: 3, maxWidth: 720, mx: 'auto', mb: 3 }}>
        <Stack direction="row" spacing={1.2} alignItems="center" sx={{ mb: 2.5 }}>
          <Box sx={{ width: 36, height: 36, borderRadius: 2.5, display: 'grid', placeItems: 'center', bgcolor: alpha('#33307A', 0.1), color: 'primary.main' }}>
            <NfcIcon fontSize="small" />
          </Box>
          <Typography variant="h6">Scan a Card</Typography>
        </Stack>
        <Grid container spacing={2} alignItems="center">
          <Grid item xs={12} sm={4}>
            <TextField
              select fullWidth label="Event" value={eventId}
              onChange={(e) => { setEventId(e.target.value); setScan(null); }}
            >
              {events.length === 0 && <MenuItem value="" disabled>No events yet</MenuItem>}
              {events.map((ev) => <MenuItem key={ev.id} value={ev.id}>{ev.name}</MenuItem>)}
            </TextField>
          </Grid>
          <Grid item xs={12} sm={5}>
            <TextField
              fullWidth label="Card ID" placeholder="Tap or type card ID" value={cardId}
              onChange={(e) => setCardId(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleScan()}
            />
          </Grid>
          <Grid item xs={12} sm={3}>
            <Button
              fullWidth variant="contained" size="large" onClick={handleScan}
              disabled={!eventId || !cardId} startIcon={<NfcIcon />}
            >
              Scan
            </Button>
          </Grid>
        </Grid>
      </Paper>

      {scan && (
        <Paper
          elevation={0}
          sx={{
            p: 4, maxWidth: 720, mx: 'auto', mb: 3, textAlign: 'center',
            border: '2px solid', borderColor: current.color,
            bgcolor: 'background.paper',
          }}
        >
          <Avatar
            src={scan.student?.image}
            sx={{
              width: 104, height: 104, mx: 'auto', mb: 2, fontSize: 36,
              border: '4px solid', borderColor: current.color,
            }}
          >
            {scan.student?.name?.[0] || '?'}
          </Avatar>
          <Typography variant="h5">{scan.student?.name || 'Unknown Card'}</Typography>
          {scan.student?.className && (
            <Typography variant="body2" color="text.secondary">{scan.student.className}</Typography>
          )}
          <Chip
            sx={{ mt: 2, px: 1, fontSize: '0.85rem', height: 34 }}
            icon={<current.Icon />}
            label={current.label}
            color={scan.status === 'allowed' ? 'success' : scan.status === 'already' ? 'error' : 'warning'}
          />
        </Paper>
      )}

      <Paper sx={{ p: { xs: 3, sm: 4 }, maxWidth: 720, mx: 'auto' }}>
        <SectionHeader icon={<HistoryOutlinedIcon fontSize="small" />} title={`History (${history.length})`} />
        {history.length === 0 ? (
          <EmptyState icon={<Inventory2OutlinedIcon sx={{ fontSize: 36, opacity: 0.4 }} />} text="No one has scanned in for this event yet." />
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow><TableCell>Image</TableCell><TableCell>Name</TableCell><TableCell>Class</TableCell></TableRow>
            </TableHead>
            <TableBody>
              {history.map((studentId) => {
                const s = students.find((st) => st.id === studentId);
                if (!s) return null;
                return (
                  <TableRow key={studentId} hover>
                    <TableCell><Avatar src={s.image} sx={{ width: 32, height: 32 }}>{s.name[0]}</Avatar></TableCell>
                    <TableCell sx={{ fontWeight: 600 }}>{s.name}</TableCell>
                    <TableCell>{s.className}</TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        )}
      </Paper>
    </Box>
  );
}