export interface AuthResponse {
    isAuthenticated: boolean;
    claims: {
        type: string;
        value: string;
    }[];
}
