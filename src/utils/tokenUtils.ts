import { jwtDecode } from 'jwt-decode'

interface DecodedToken {
  exp: number;
  iat: number;
  email: string;
  name: string;
  IsAdmin: string;
}

export const parseJwt = (token: string): DecodedToken | null => {
  try {
    return jwtDecode<DecodedToken>(token)
  } catch {
    return null
  }
}

export const isTokenExpired = (token: string): boolean => {
  try {
    const decoded = jwtDecode<DecodedToken>(token);
    const currentTime = Date.now() / 1000;
    return decoded.exp < currentTime;
  } catch {
    return true;
  }
}

export const getTokenExpirationTime = (token: string): number | null => {
  try {
    const decoded = jwtDecode<DecodedToken>(token);
    return decoded.exp;
  } catch {
    return null;
  }
}