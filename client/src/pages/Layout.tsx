import { Routes, Route, useLocation } from 'react-router-dom';
import { Box } from '@mui/material';
import { Navbar } from '../components/Navbar';
import { TasksTable } from './TasksTable';
import { NewTaskForm } from './NewTaskForm';
import { NewUserForm } from './NewUserForm';
import { NewTagForm } from './NewTagForm';

type View = 'tasks' | 'newTask' | 'newUser' | 'newTag';

const viewMap: Record<string, View> = {
  '/': 'tasks',
  '/new-task': 'newTask',
  '/new-user': 'newUser',
  '/new-tag': 'newTag',
};

export function Layout() {
  const location = useLocation();
  const currentView = viewMap[location.pathname] || 'tasks';
  const isTasksView = location.pathname === '/';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Navbar currentView={currentView} />

      <Box
        sx={{
          mt: isTasksView ? 0 : 4,
          mb: isTasksView ? 0 : 4,
          px: isTasksView ? 0 : { xs: 2, sm: 3 },
          flexGrow: 1,
          width: '100%',
          maxWidth: isTasksView ? '100%' : '1200px',
          mx: 'auto',
        }}
      >
        <Routes>
          <Route path="/" element={<TasksTable />} />
          <Route path="/new-task" element={<NewTaskForm />} />
          <Route path="/new-user" element={<NewUserForm />} />
          <Route path="/new-tag" element={<NewTagForm />} />
        </Routes>
      </Box>
    </Box>
  );
}
