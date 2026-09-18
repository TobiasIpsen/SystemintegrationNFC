import React, { useState } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  IconButton, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Button, Avatar, Tooltip, Alert, Stack,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import AddIcon from '@mui/icons-material/Add';
import EventOutlinedIcon from '@mui/icons-material/EventOutlined';
import GroupsOutlinedIcon from '@mui/icons-material/GroupsOutlined';
import SectionHeader from './SectionHeader.jsx';
import EmptyState from './EmptyState.jsx';
import StudentForm from './StudentForm.jsx';
import * as api from '../api.js';
import { uid, emptyEvent, makeEmptyStudent, resolveImageUrl } from '../utils/helpers.js';

export default function AdminPanel({ students, events, onRefresh }) {
  const [studentDialog, setStudentDialog] = useState(null);
  const [eventDialog, setEventDialog] = useState(null);
  const [dialogError, setDialogError] = useState('');

  const saveStudent = async () => {
    setDialogError('');
    const exists = students.some((s) => s.id === studentDialog.student.id);
    try {
      const image = await resolveImageUrl(studentDialog.student);
      const payload = { ...studentDialog.student, image };
      if (exists) await api.updateStudent(payload);
      else await api.createStudent(payload);
      await onRefresh();
      setStudentDialog(null);
    } catch (err) {
      setDialogError(err.message || 'Could not save student.');
    }
  };

  const saveEvent = async () => {
    setDialogError('');
    const exists = events.some((e) => e.id === eventDialog.event.id);
    try {
      if (exists) await api.updateEvent(eventDialog.event);
      else await api.createEvent(eventDialog.event);
      await onRefresh();
      setEventDialog(null);
    } catch (err) {
      setDialogError(err.message || 'Could not save event.');
    }
  };

  const removeStudent = async (id) => {
    await api.deleteStudent(id);
    await onRefresh();
  };

  const removeEvent = async (id) => {
    await api.deleteEvent(id);
    await onRefresh();
  };

  return (
    <Stack spacing={4} sx={{ maxWidth: 920, mx: 'auto' }}>
      {/* Events */}
      <Paper sx={{ p: { xs: 3, sm: 4 } }}>
        <SectionHeader
          icon={<EventOutlinedIcon fontSize="small" />}
          title={`Events (${events.length})`}
          action={
            <Button
              size="small" startIcon={<AddIcon />}
              onClick={() => { setDialogError(''); setEventDialog({ event: { ...emptyEvent, id: uid() } }); }}
            >
              New Event
            </Button>
          }
        />
        {events.length === 0 ? (
          <EmptyState icon={<EventOutlinedIcon sx={{ fontSize: 36, opacity: 0.4 }} />} text="No events yet — create your first one." />
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow><TableCell>Name</TableCell><TableCell align="right">Actions</TableCell></TableRow>
            </TableHead>
            <TableBody>
              {events.map((ev) => (
                <TableRow key={ev.id} hover>
                  <TableCell sx={{ fontWeight: 600 }}>{ev.name}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => { setDialogError(''); setEventDialog({ event: { ...ev } }); }}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete">
                      <IconButton size="small" color="error" onClick={() => removeEvent(ev.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Paper>

      {/* Users */}
      <Paper sx={{ p: { xs: 3, sm: 4 } }}>
        <SectionHeader
          icon={<GroupsOutlinedIcon fontSize="small" />}
          title={`Users (${students.length})`}
          action={
            <Button
              size="small" startIcon={<AddIcon />}
              onClick={() => { setDialogError(''); setStudentDialog({ student: makeEmptyStudent() }); }}
            >
              New User
            </Button>
          }
        />
        {students.length === 0 ? (
          <EmptyState icon={<GroupsOutlinedIcon sx={{ fontSize: 36, opacity: 0.4 }} />} text="No students yet — add one from the User Panel or here." />
        ) : (
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Image</TableCell><TableCell>Name</TableCell>
                <TableCell>Class</TableCell><TableCell>Card ID</TableCell>
                <TableCell>Events</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {students.map((s) => (
                <TableRow key={s.id} hover>
                  <TableCell><Avatar src={s.image} sx={{ width: 34, height: 34 }}>{s.name[0]}</Avatar></TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>{s.name}</TableCell>
                  <TableCell>{s.className}</TableCell>
                  <TableCell>{s.cardId}</TableCell>
                  <TableCell>{s.eventIds.length}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Edit">
                      <IconButton size="small" onClick={() => { setDialogError(''); setStudentDialog({ student: { ...s } }); }}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Delete">
                      <IconButton size="small" color="error" onClick={() => removeStudent(s.id)}>
                        <DeleteOutlineIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </Paper>

      {/* Student dialog */}
      <Dialog open={!!studentDialog} onClose={() => setStudentDialog(null)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>{studentDialog?.student ? 'Edit User' : 'New User'}</DialogTitle>
        <DialogContent sx={{ pt: '20px !important' }}>
          {dialogError && <Alert severity="error" sx={{ mb: 2 }}>{dialogError}</Alert>}
          {studentDialog && (
            <StudentForm
              value={studentDialog.student}
              onChange={(student) => setStudentDialog({ student })}
              events={events}
            />
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2.5, pt: 1 }}>
          <Button onClick={() => setStudentDialog(null)} color="inherit">Cancel</Button>
          <Button variant="contained" onClick={saveStudent}>Save</Button>
        </DialogActions>
      </Dialog>

      {/* Event dialog */}
      <Dialog open={!!eventDialog} onClose={() => setEventDialog(null)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>{eventDialog?.event ? 'Edit Event' : 'New Event'}</DialogTitle>
        <DialogContent sx={{ pt: '20px !important' }}>
          {dialogError && <Alert severity="error" sx={{ mb: 2 }}>{dialogError}</Alert>}
          {eventDialog && (
            <TextField
              label="Event Name" fullWidth autoFocus value={eventDialog.event.name}
              onChange={(e) => setEventDialog({ event: { ...eventDialog.event, name: e.target.value } })}
            />
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2.5, pt: 1 }}>
          <Button onClick={() => setEventDialog(null)} color="inherit">Cancel</Button>
          <Button variant="contained" onClick={saveEvent}>Save</Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}