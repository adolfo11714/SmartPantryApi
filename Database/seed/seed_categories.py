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


def ensure_categories_table(conn: MySQLConnection) -> None:
    ddl = (
        """
        CREATE TABLE IF NOT EXISTS `categories` (
          `ID` INT NOT NULL,
          `Category_Name` VARCHAR(255) NOT NULL,
          `Subcategory_Name` VARCHAR(255) NULL,
          PRIMARY KEY (`ID`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """
    )
    with conn.cursor() as cur:
        cur.execute(ddl)


def normalize_record(raw_list: List[Dict[str, Any]]) -> Dict[str, Any]:
    # Input per category is a list of single-key objects -> flatten
    merged: Dict[str, Any] = {}
    for obj in raw_list:
        for k, v in obj.items():
            merged[k] = v
    # Convert floats like 1.0 -> int
    for key in list(merged.keys()):
        val = merged[key]
        if isinstance(val, float) and val.is_integer():
            merged[key] = int(val)
    return merged


def upsert_categories(conn: MySQLConnection, categories: List[List[Dict[str, Any]]]) -> int:
    insert_sql = (
        """
        INSERT IGNORE INTO `categories` (
          `ID`, `Category_Name`, `Subcategory_Name`
        ) VALUES (
          %(ID)s, %(Category_Name)s, %(Subcategory_Name)s
        );
        """
    )

    count = 0
    with conn.cursor() as cur:
        for raw in categories:
            rec = normalize_record(raw)
            # Ensure all expected keys are present (None if missing)
            keys = ["ID", "Category_Name", "Subcategory_Name"]
            params = {k: rec.get(k) for k in keys}
            cur.execute(insert_sql, params)
            count += 1
    return count


def main() -> None:
    data = read_json("category.json")
    categories = data.get("data", [])

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
        ensure_categories_table(conn)
        inserted = upsert_categories(conn, categories)
        print(f"Processed {inserted} categories (existing IDs ignored).")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
