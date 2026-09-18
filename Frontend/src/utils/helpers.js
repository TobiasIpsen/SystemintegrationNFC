import * as api from "../api.js";

export const uid = () => Math.random().toString(36).slice(2, 9);

export const emptyEvent = { name: "" };

export const makeEmptyStudent = () => ({
  name: "",
  className: "",
  cardId: "",
  image: "",
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

export async function resolveImageUrl(student) {
  if (student.image && student.image.startsWith("data:")) {
    const { blob, contentType } = dataUrlToBlob(student.image);
    const { uploadUrl, objectKey, imageGuid } = await api.fetchUploadUrl();
    await api.uploadToPresignedUrl(uploadUrl, blob, contentType);
    return { objectKey, imageGuid };
  }

  // If image already exists (is a URL/GUID), return it as-is
  if (student.image) {
    return student.image;
  }

  // No image provided
  throw new Error("Please take a photo");
}
