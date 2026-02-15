export interface TagValidationErrors {
  name?: string;
}

export const validateTag = (name: string, existingTags: Array<{ id: number; name: string }> = []): TagValidationErrors => {
  const errors: TagValidationErrors = {};

  if (!name.trim()) {
    errors.name = 'Name is required';
  } else if (name.trim().length < 2) {
    errors.name = 'Name must be at least 2 characters';
  } else if (name.trim().length > 50) {
    errors.name = 'Name cannot exceed 50 characters';
  } else if (existingTags.some(tag => tag.name.toLowerCase() === name.trim().toLowerCase())) {
    errors.name = 'A tag with this name already exists';
  }

  return errors;
};