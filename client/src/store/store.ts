import { configureStore } from '@reduxjs/toolkit'
import { tasksApi } from '../services/tasksApi'
import { tagsApi } from '../services/tagsApi'
import { usersApi } from '../services/usersApi'

export const store = configureStore({
  reducer: {
    [tasksApi.reducerPath]: tasksApi.reducer,
    [tagsApi.reducerPath]: tagsApi.reducer,
    [usersApi.reducerPath]: usersApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware()
      .concat(tasksApi.middleware)
      .concat(tagsApi.middleware)
      .concat(usersApi.middleware),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch