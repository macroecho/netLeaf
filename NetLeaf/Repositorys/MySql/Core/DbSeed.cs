namespace NetLeaf.Repositorys.MySql.Core
{
    internal class DbSeed
    {
        internal const string Script =
        @"
             CREATE DATABASE IF NOT EXISTS netleaf;

             USE netleaf;
             
             CREATE TABLE IF NOT EXISTS leaf_segment
             (
                 `max_id` BIGINT NOT NULL,
                 `step`   INT    NOT NULL,
                 `time`   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
             ) ENGINE=InnoDB;
             
             INSERT INTO leaf_segment(`max_id`, `step`) SELECT 0, 1000 FROM DUAL WHERE NOT EXISTS(SELECT * FROM leaf_segment);
        ";
    }
}
