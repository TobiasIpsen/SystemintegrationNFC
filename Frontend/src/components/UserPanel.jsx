import React, { useState } from "react";
import { Paper, Button, Divider, Alert, Stack, Chip } from "@mui/material";
import PersonAddAltIcon from "@mui/icons-material/PersonAddAlt";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import SectionHeader from "./SectionHeader.jsx";
import StudentForm from "./StudentForm.jsx";
import { makeEmptyStudent, resolveImageUrl } from "../utils/helpers.js";
import * as api from '../api.js';

export default function UserPanel({ events }) {
  const [form, setForm] = useState(makeEmptyStudent);
  const [justAdded, setJustAdded] = useState(false);
  const [error, setError] = useState("");

  const handleCreateStudent = async (student) => {
    await api.createStudent(student);
    await refresh();
  };

  const submit = async () => {
    if (!form.name || !form.cardId) return;
    setError("");
    try {
      const image = await resolveImageUrl(form);
      await handleCreateStudent({ ...form, image });
      setForm(makeEmptyStudent());
      setJustAdded(true);
      setTimeout(() => setJustAdded(false), 2500);
    } catch (err) {
      console.log(err);
      
      setError(err.message || "Could not create student.");
    }
  };

  return (
    <Paper sx={{ p: { xs: 3, sm: 4 }, maxWidth: 680, mx: "auto" }}>
      <SectionHeader
        icon={<PersonAddAltIcon fontSize="small" />}
        title="Create New Student"
      />
      <StudentForm value={form} onChange={setForm} events={events} />
      <Divider sx={{ my: 3 }} />
      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}
      <Stack direction="row" alignItems="center" spacing={2}>
        <Button
          variant="contained"
          size="large"
          onClick={submit}
          disabled={!form.name || !form.cardId}
          startIcon={<PersonAddAltIcon />}
        >
          Create Student
        </Button>
        {justAdded && (
          <Chip
            color="success"
            icon={<CheckCircleIcon />}
            label="Student created"
            variant="outlined"
          />
        )}
      </Stack>
    </Paper>
  );
}
