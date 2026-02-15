import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateTagMutation, useGetAllTagsQuery } from '../services/tagsApi';
import { validateTag, type TagValidationErrors } from '../validation/tagValidation';
import { getErrorMessage } from '../types/api';
import { TextField } from '@mui/material';
import { BaseForm } from '../components/BaseForm';
import { FormAlerts } from '../components/FormAlerts';
import { FormActions } from '../components/FormActions';

export function NewTagForm() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [errors, setErrors] = useState<TagValidationErrors>({});

  const [createTag, { isLoading, isSuccess, isError, error }] = useCreateTagMutation();
  const { data: tags = [] } = useGetAllTagsQuery();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = validateTag(name, tags);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    setErrors({});

    try {
      await createTag({ name }).unwrap();
      setName('');
      navigate('/');
    } catch (err) {
      // Error is handled by RTK Query
    }
  };

  const handleClear = () => {
    setName('');
    setErrors({});
  };

  return (
    <BaseForm title="Create New Tag" onSubmit={handleSubmit} maxWidth={400}>
      <FormAlerts
        isSuccess={isSuccess}
        isError={isError}
        successMessage="Tag created successfully!"
        errorMessage={`Failed to create tag: ${getErrorMessage(error)}`}
      />

      <TextField
        fullWidth
        label="Name"
        value={name}
        onChange={(e) => setName(e.target.value)}
        error={!!errors.name}
        helperText={errors.name || `${name.trim().length}/50 characters (min 2)`}
        margin="normal"
        required
        disabled={isLoading}
        slotProps={{ htmlInput: { maxLength: 50 } }}
      />

      <FormActions
        isLoading={isLoading}
        submitLabel="Create Tag"
        loadingLabel="Creating..."
        onClear={handleClear}
        onCancel={() => navigate('/')}
      />
    </BaseForm>
  );
}
