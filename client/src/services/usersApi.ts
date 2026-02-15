import { baseApi } from "./baseApi";
import type { UserDto, CreateUserDto, UpdateUserDto } from "../types/dtos";

export const usersApi = baseApi.injectEndpoints({
  endpoints: (builder) => ({
    getAllUsers: builder.query<UserDto[], void>({
      query: () => "/users",
      providesTags: ["User"],
    }),

    getUserById: builder.query<UserDto, number>({
      query: (id) => `/users/${id}`,
      // @ts-ignore - unused params required by RTK Query signature
      providesTags: (_, __, id) => [{ type: "User", id }],
    }),

    createUser: builder.mutation<UserDto, CreateUserDto>({
      query: (body) => ({
        url: "/users",
        method: "POST",
        body,
      }),
      invalidatesTags: ["User"],
    }),

    updateUser: builder.mutation<UserDto, { id: number; body: UpdateUserDto }>({
      query: ({ id, body }) => ({
        url: `/users/${id}`,
        method: "PUT",
        body,
      }),
      // @ts-ignore - unused params required by RTK Query signature
      invalidatesTags: (_, __, { id }) => [{ type: "User", id }, "User"],
    }),

    deleteUser: builder.mutation<void, number>({
      query: (id) => ({
        url: `/users/${id}`,
        method: "DELETE",
      }),
      invalidatesTags: ["User"],
    }),
  }),
});

export const {
  useGetAllUsersQuery,
  useGetUserByIdQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useDeleteUserMutation,
} = usersApi;