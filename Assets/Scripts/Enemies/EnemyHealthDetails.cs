    // We can differentiate enemy Heath Amount among levels.
    [System.Serializable]
    public struct EnemyHeathDetails
    {
        public DungeonLevelSO dungeonLevel;
        public int enemyHealthAmount;
    }