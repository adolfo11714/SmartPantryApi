import os
import sys
import time
from typing import Any, Dict, List

import orjson
import mysql.connector
from mysql.connector.connection import MySQLConnection


def read_json(path: str) -> Dict[str, Any]:
    with open(path, "rb") as f:
        return orjson.loads(f.read())


def get_connection() -> MySQLConnection:
    conn = mysql.connector.connect(
        host=os.environ.get("DB_HOST", "localhost"),
        port=int(os.environ.get("DB_PORT", "3306")),
        database=os.environ.get("DB_NAME", "food_pantry"),
        user=os.environ.get("DB_USER", "root"),
        password=os.environ.get("DB_PASSWORD", "rootpass"),
        autocommit=True,
    )
    return conn


def ensure_table(conn: MySQLConnection) -> None:
    ddl = (
        """
        CREATE TABLE IF NOT EXISTS `versions` (
          `Data_Version_Number` DECIMAL(10,1) NOT NULL,
          `Current_Version` VARCHAR(10) NULL,
          `Modified_Date` VARCHAR(20) NULL,
          `FSIS_Approved_Flag` VARCHAR(10) NULL,
          `Approved_Date` VARCHAR(20) NULL,
          `Notes` TEXT NULL,
          PRIMARY KEY (`Data_Version_Number`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """
    )
    with conn.cursor() as cur:
        cur.execute(ddl)


def normalize_record(raw_list: List[Dict[str, Any]]) -> Dict[str, Any]:
    # Input per version is a list of single-key objects -> flatten
    merged: Dict[str, Any] = {}
    for obj in raw_list:
        for k, v in obj.items():
            merged[k] = v
    return merged


def upsert_versions(conn: MySQLConnection, versions: List[List[Dict[str, Any]]]) -> int:
    insert_sql = (
        """
        INSERT IGNORE INTO `versions` (
          `Data_Version_Number`, `Current_Version`, `Modified_Date`, 
          `FSIS_Approved_Flag`, `Approved_Date`, `Notes`
        ) VALUES (
          %(Data_Version_Number)s, %(Current_Version)s, %(Modified_Date)s, 
          %(FSIS_Approved_Flag)s, %(Approved_Date)s, %(Notes)s
        );
        """
    )

    count = 0
    with conn.cursor() as cur:
        for raw in versions:
            rec = normalize_record(raw)
            # Ensure all expected keys are present (None if missing)
            keys = [
                "Data_Version_Number", "Current_Version", "Modified_Date", 
                "FSIS_Approved_Flag", "Approved_Date", "Notes"
            ]
            params = {k: rec.get(k) for k in keys}
            cur.execute(insert_sql, params)
            count += 1
    return count


def main() -> None:
    data = read_json("version.json")
    versions = data.get("data", [])

    # Retry connection until MySQL is ready (compose healthcheck should handle it, but be safe)
    for i in range(30):
        try:
            conn = get_connection()
            break
        except Exception as e:
            time.sleep(2)
    else:
        print("Failed to connect to MySQL", file=sys.stderr)
        sys.exit(1)

    try:
        ensure_table(conn)
        inserted = upsert_versions(conn, versions)
        print(f"Processed {inserted} versions (existing version numbers ignored).")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
