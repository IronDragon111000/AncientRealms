namespace AncientRealms.Helpers
{
    public static class MovementHelper
    {
        public static Vector2 AdjustAim(float AimSpeed, Vector2 CurrentDirection, Vector2 Aim)
        {
            if (Aim.HasNaNs())
            {
                Aim = -Vector2.UnitY;
            }

            // Calculate current and target angles
            float currentAngle = CurrentDirection.ToRotation();
            float targetAngle = Aim.ToRotation();

            // Get the smallest angle difference
            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);

            // Rotate by a constant amount towards the target, clamped to max speed
            float turnAmount = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
            float newAngle = currentAngle + turnAmount;

            // Set new AttackDirection
            return newAngle.ToRotationVector2();
        }
        public static float AdjustAim(float AimSpeed, float CurrentDirection, float Aim)
        {
            // Get the smallest angle difference
            float angleDiff = MathHelper.WrapAngle(Aim - CurrentDirection);

            // Rotate by a constant amount towards the target, clamped to max speed
            float turnAmount = MathHelper.Clamp(angleDiff, -AimSpeed, AimSpeed);
            return CurrentDirection + turnAmount;
        }
    }
}