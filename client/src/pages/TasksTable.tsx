import { useState, useCallback } from 'react';
import { useGetAllTasksQuery, useDeleteTaskMutation } from '../services/tasksApi';
import { useGetAllUsersQuery } from '../services/usersApi';
import { getErrorMessage } from '../types/api';
import {
  DataGrid,
  GridActionsCellItem,
} from '@mui/x-data-grid';
import type { GridColDef, GridRowId } from '@mui/x-data-grid';
import {
  Box,
  Typography,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Chip,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { EditTaskForm } from './EditTaskForm';

export function TasksTable() {
  const [editingTaskId, setEditingTaskId] = useState<number | null>(null);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [taskToDelete, setTaskToDelete] = useState<number | null>(null);

  const { data: tasks, isLoading, error } = useGetAllTasksQuery();
  const { data: users } = useGetAllUsersQuery();
  const [deleteTask, { isLoading: isDeleting }] = useDeleteTaskMutation();

  const handleEdit = useCallback((id: GridRowId) => {
    setEditingTaskId(Number(id));
  }, []);

  const handleDeleteClick = useCallback((id: GridRowId) => {
    setTaskToDelete(Number(id));
    setDeleteConfirmOpen(true);
  }, []);

  const handleDeleteConfirm = async () => {
    if (taskToDelete) {
      try {
        await deleteTask(taskToDelete).unwrap();
        setDeleteConfirmOpen(false);
        setTaskToDelete(null);
      } catch (err) {
        // Error handled by RTK Query
      }
    }
  };

  const handleEditClose = useCallback(() => {
    setEditingTaskId(null);
  }, []);

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 70, type: 'number' },
    { field: 'title', headerName: 'Title', width: 200 },
    { field: 'description', headerName: 'Description', width: 300 },
    {
      field: 'dueDate',
      headerName: 'Due Date',
      width: 120,
      type: 'date',
      valueGetter: (value) => new Date(value as string),
      valueFormatter: (value) => (value as Date).toLocaleDateString(),
    },
    {
      field: 'priority',
      headerName: 'Priority',
      width: 100,
      type: 'singleSelect',
      valueOptions: [
        { value: 1, label: 'Low' },
        { value: 2, label: 'Medium' },
        { value: 3, label: 'High' },
      ],
      valueFormatter: (value) => {
        const priorities = { 1: 'Low', 2: 'Medium', 3: 'High' };
        return priorities[value as keyof typeof priorities] || value;
      },
    },
    {
      field: 'userId',
      headerName: 'User',
      width: 200,
      valueFormatter: (value) => {
        const user = users?.find(u => u.id === (value as number));
        return user ? `${user.fullName} (${user.email})` : value;
      },
    },
    {
      field: 'tags',
      headerName: 'Tags',
      width: 200,
      valueFormatter: (value) => {
        const tagNames = (value as any[]).map((tag: any) => tag.name).join(', ');
        return tagNames;
      },
      renderCell: (params) => (
        <Box>
          {(params.value as any[]).map((tag: any) => (
            <Chip key={tag.id} label={tag.name} size="small" sx={{ mr: 0.5 }} />
          ))}
        </Box>
      ),
    },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Actions',
      width: 100,
      renderCell: (params) => (
        <>
          <GridActionsCellItem
            icon={<EditIcon />}
            label="Edit"
            onClick={() => handleEdit(params.id)}
          />
          <GridActionsCellItem
            icon={<DeleteIcon />}
            label="Delete"
            onClick={() => handleDeleteClick(params.id)}
          />
        </>
      ),
    },
  ];

  if (error) {
    return (
      <Alert severity="error">
        Failed to load tasks: {getErrorMessage(error)}
      </Alert>
    );
  }

  return (
    <Box sx={{ width: '100%', p: { xs: 2, sm: 3 } }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Tasks
      </Typography>

      <Box sx={{ height: 'calc(100vh - 250px)', width: '100%' }}>
        <DataGrid
        rows={tasks || []}
        columns={columns}
        loading={isLoading}
        disableRowSelectionOnClick
        initialState={{
          pagination: {
            paginationModel: { page: 0, pageSize: 10 },
          },
          sorting: {
            sortModel: [{ field: 'id', sort: 'asc' }],
          },
        }}
        pageSizeOptions={[5, 10, 25]}
        sx={{
          '& .MuiDataGrid-cell': {
            overflow: 'hidden',
            textOverflow: 'ellipsis',
          },
        }}
      />
      </Box>

      {/* Edit Dialog */}
      <Dialog
        open={editingTaskId !== null}
        onClose={handleEditClose}
        maxWidth="md"
        fullWidth
      >
        <DialogTitle>Edit Task</DialogTitle>
        <DialogContent>
          {editingTaskId && (
            <EditTaskForm
              taskId={editingTaskId}
              onSuccess={handleEditClose}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleEditClose}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteConfirmOpen}
        onClose={() => setDeleteConfirmOpen(false)}
      >
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this task? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Cancel</Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            disabled={isDeleting}
          >
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
