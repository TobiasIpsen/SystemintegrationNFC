import * as api from "../api.js";

export const uid = () => Math.random().toString(36).slice(2, 9);

export const emptyEvent = { name: "" };

export const makeEmptyStudent = () => ({
  id: uid(),
  name: "",
  photo: "",
  cardId: "",
  className: "",
  eventIds: [],
});

function dataUrlToBlob(dataUrl) {
  const [header, base64Data] = dataUrl.split(",");
  const mimeMatch = header.match(/:(.*?);/);
  const contentType = mimeMatch ? mimeMatch[1] : "image/jpeg";

  const binaryStr = atob(base64Data);
  const array = new Uint8Array(binaryStr.length);
  for (let i = 0; i < binaryStr.length; i++) {
    array[i] = binaryStr.charCodeAt(i);
  }

  return { blob: new Blob([array], { type: contentType }), contentType };
}

export async function resolvePhotoUrl(student) {
  if (!student.photo || !student.photo.startsWith("data:"))
    return student.photo || null;

  const { blob, contentType } = dataUrlToBlob(student.photo);
  console.log(contentType);
  
  const fileName = `${student.id}.jpg`
  const { uploadUrl, objectKey } = await api.fetchUploadUrl(fileName, contentType);
  await api.uploadToPresignedUrl(uploadUrl, blob, contentType);
  await markSynced(student.id);
  return objectKey;
}
