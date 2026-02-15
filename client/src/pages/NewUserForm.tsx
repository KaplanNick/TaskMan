import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCreateUserMutation, useGetAllUsersQuery } from '../services/usersApi';
import { validateUser, type UserValidationErrors } from '../validation/userValidation';
import { getErrorMessage } from '../types/api';
import { TextField } from '@mui/material';
import { BaseForm } from '../components/BaseForm';
import { FormAlerts } from '../components/FormAlerts';
import { FormActions } from '../components/FormActions';

export function NewUserForm() {
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [telephone, setTelephone] = useState('');
  const [errors, setErrors] = useState<UserValidationErrors>({});

  const [createUser, { isLoading, isSuccess, isError, error }] = useCreateUserMutation();
  const { data: existingUsers = [] } = useGetAllUsersQuery();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const validationErrors = validateUser(fullName, email, telephone);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    // Check for duplicate email or telephone before submitting
    const duplicateErrors: UserValidationErrors = {};
    
    const emailExists = existingUsers.some(
      user => user.email.toLowerCase() === email.toLowerCase()
    );
    if (emailExists) {
      duplicateErrors.email = `A user with the email '${email}' already exists.`;
    }

    const telephoneExists = existingUsers.some(
      user => user.telephone === telephone
    );
    if (telephoneExists) {
      duplicateErrors.telephone = `A user with the telephone '${telephone}' already exists.`;
    }

    if (Object.keys(duplicateErrors).length > 0) {
      setErrors(duplicateErrors);
      return;
    }

    setErrors({});

    try {
      await createUser({ fullName, email, telephone }).unwrap();
      setFullName('');
      setEmail('');
      setTelephone('');
      navigate('/');
    } catch (err) {
      // Map error message to the correct field based on error content
      const errorMessage = getErrorMessage(err);
      const mappedErrors: any = {};

      if (errorMessage.toLowerCase().includes('email')) {
        mappedErrors.email = errorMessage;
      } else if (errorMessage.toLowerCase().includes('telephone')) {
        mappedErrors.telephone = errorMessage;
      } else {
        mappedErrors.fullName = errorMessage;
      }

      setErrors(prev => ({ ...prev, ...mappedErrors }));
    }
  };

  const handleClear = () => {
    setFullName('');
    setEmail('');
    setTelephone('');
    setErrors({});
  };

  return (
    <BaseForm title="Create New User" onSubmit={handleSubmit} maxWidth={400}>
      <FormAlerts
        isSuccess={isSuccess}
        isError={isError}
        successMessage="User created successfully!"
        errorMessage={`Failed to create user: ${getErrorMessage(error)}`}
      />

      <TextField
        fullWidth
        label="Full Name"
        value={fullName}
        onChange={(e) => setFullName(e.target.value)}
        error={!!errors.fullName}
        helperText={errors.fullName}
        margin="normal"
        required
        disabled={isLoading}
      />

      <TextField
        fullWidth
        label="Email"
        type="email"
        value={email}
        onChange={(e) => setEmail(e.target.value)}
        error={!!errors.email}
        helperText={errors.email}
        margin="normal"
        required
        disabled={isLoading}
      />

      <TextField
        fullWidth
        label="Telephone"
        value={telephone}
        onChange={(e) => setTelephone(e.target.value)}
        error={!!errors.telephone}
        helperText={errors.telephone}
        margin="normal"
        required
        disabled={isLoading}
      />

      <FormActions
        isLoading={isLoading}
        submitLabel="Create User"
        loadingLabel="Creating..."
        onClear={handleClear}
        onCancel={() => navigate('/')}
      />
    </BaseForm>
  );
}
