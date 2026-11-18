/**
 * Client-side mirror of the backend password policy (UserService.ValidatePasswordPolicy):
 * at least 8 characters with an uppercase letter, a lowercase letter and a digit.
 * Returns an error message, or null when the password satisfies the policy.
 */
export function getPasswordPolicyError(password: string | null | undefined): string | null {
  if (!password || !password.trim()) {
    return 'Password is required.';
  }
  if (password.length < 8) {
    return 'Password must be at least 8 characters long.';
  }
  if (!/[A-Z]/.test(password)) {
    return 'Password must contain at least one uppercase letter.';
  }
  if (!/[a-z]/.test(password)) {
    return 'Password must contain at least one lowercase letter.';
  }
  if (!/[0-9]/.test(password)) {
    return 'Password must contain at least one digit.';
  }
  return null;
}
