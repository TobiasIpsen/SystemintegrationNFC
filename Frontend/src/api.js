async function request(path, options = {}) {
  const res = await fetch(`http://localhost:5233/api${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.error || `Request failed (${res.status})`);
  }
  return res.status === 204 ? null : res.json();
}

export const fetchUploadUrl = (fileName, contentType) =>
  request('/file/generate-upload-url', { method: "POST", body: JSON.stringify({fileName, contentType})})

export const uploadToPresignedUrl = async (uploadUrl, blob, contentType) => {
  const res = await fetch(uploadUrl, {
    method: "PUT",
    headers: { "Content-Type": "host"},
    body: blob,
  });

  if (!res.ok) {
    throw new Error(`Direct SeaweedFS upload failed (${res.status})`)
  }
};

export const fetchStudents = () => request('/students');
export const createStudent = (student) =>
  request('/students', { method: 'POST', body: JSON.stringify(student) });
export const updateStudent = (student) =>
  request(`/students/${student.id}`, { method: 'PUT', body: JSON.stringify(student) });
export const deleteStudent = (id) => request(`/students/${id}`, { method: 'DELETE' });

export const fetchEvents = () => request('/events');
export const createEvent = (event) =>
  request('/events', { method: 'POST', body: JSON.stringify(event) });
export const updateEvent = (event) =>
  request(`/events/${event.id}`, { method: 'PUT', body: JSON.stringify(event) });
export const deleteEvent = (id) => request(`/events/${id}`, { method: 'DELETE' });

export const fetchAccessLog = (eventId) => request(`/access-log/${eventId}`);
export const logAccess = (eventId, studentId) =>
  request('/access-log', { method: 'POST', body: JSON.stringify({ eventId, studentId }) });