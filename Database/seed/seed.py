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
        CREATE TABLE IF NOT EXISTS `items` (
          `ID` INT NOT NULL,
          `Category_ID` INT NULL,
          `Name` VARCHAR(255) NOT NULL,
          `Name_subtitle` VARCHAR(255) NULL,
          `Keywords` VARCHAR(255) NULL,
          `Pantry_Min` INT NULL,
          `Pantry_Max` INT NULL,
          `Pantry_Metric` VARCHAR(64) NULL,
          `Pantry_tips` TEXT NULL,
          `DOP_Pantry_Min` INT NULL,
          `DOP_Pantry_Max` INT NULL,
          `DOP_Pantry_Metric` VARCHAR(64) NULL,
          `DOP_Pantry_tips` TEXT NULL,
          `Pantry_After_Opening_Min` INT NULL,
          `Pantry_After_Opening_Max` INT NULL,
          `Pantry_After_Opening_Metric` VARCHAR(64) NULL,
          `Refrigerate_Min` INT NULL,
          `Refrigerate_Max` INT NULL,
          `Refrigerate_Metric` VARCHAR(64) NULL,
          `Refrigerate_tips` TEXT NULL,
          `DOP_Refrigerate_Min` INT NULL,
          `DOP_Refrigerate_Max` INT NULL,
          `DOP_Refrigerate_Metric` VARCHAR(64) NULL,
          `DOP_Refrigerate_tips` TEXT NULL,
          `Refrigerate_After_Opening_Min` INT NULL,
          `Refrigerate_After_Opening_Max` INT NULL,
          `Refrigerate_After_Opening_Metric` VARCHAR(64) NULL,
          `Refrigerate_After_Thawing_Min` INT NULL,
          `Refrigerate_After_Thawing_Max` INT NULL,
          `Refrigerate_After_Thawing_Metric` VARCHAR(64) NULL,
          `Freeze_Min` INT NULL,
          `Freeze_Max` INT NULL,
          `Freeze_Metric` VARCHAR(64) NULL,
          `Freeze_Tips` TEXT NULL,
          `DOP_Freeze_Min` INT NULL,
          `DOP_Freeze_Max` INT NULL,
          `DOP_Freeze_Metric` VARCHAR(64) NULL,
          `DOP_Freeze_Tips` TEXT NULL,
          PRIMARY KEY (`ID`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        """
    )
    with conn.cursor() as cur:
        cur.execute(ddl)


def normalize_record(raw_list: List[Dict[str, Any]]) -> Dict[str, Any]:
    # Input per product is a list of single-key objects -> flatten
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


def upsert_items(conn: MySQLConnection, products: List[List[Dict[str, Any]]]) -> int:
    insert_sql = (
        """
        INSERT IGNORE INTO `items` (
          `ID`,`Category_ID`,`Name`,`Name_subtitle`,`Keywords`,`Pantry_Min`,`Pantry_Max`,`Pantry_Metric`,`Pantry_tips`,
          `DOP_Pantry_Min`,`DOP_Pantry_Max`,`DOP_Pantry_Metric`,`DOP_Pantry_tips`,
          `Pantry_After_Opening_Min`,`Pantry_After_Opening_Max`,`Pantry_After_Opening_Metric`,
          `Refrigerate_Min`,`Refrigerate_Max`,`Refrigerate_Metric`,`Refrigerate_tips`,
          `DOP_Refrigerate_Min`,`DOP_Refrigerate_Max`,`DOP_Refrigerate_Metric`,`DOP_Refrigerate_tips`,
          `Refrigerate_After_Opening_Min`,`Refrigerate_After_Opening_Max`,`Refrigerate_After_Opening_Metric`,
          `Refrigerate_After_Thawing_Min`,`Refrigerate_After_Thawing_Max`,`Refrigerate_After_Thawing_Metric`,
          `Freeze_Min`,`Freeze_Max`,`Freeze_Metric`,`Freeze_Tips`,
          `DOP_Freeze_Min`,`DOP_Freeze_Max`,`DOP_Freeze_Metric`,`DOP_Freeze_Tips`
        ) VALUES (
          %(ID)s,%(Category_ID)s,%(Name)s,%(Name_subtitle)s,%(Keywords)s,%(Pantry_Min)s,%(Pantry_Max)s,%(Pantry_Metric)s,%(Pantry_tips)s,
          %(DOP_Pantry_Min)s,%(DOP_Pantry_Max)s,%(DOP_Pantry_Metric)s,%(DOP_Pantry_tips)s,
          %(Pantry_After_Opening_Min)s,%(Pantry_After_Opening_Max)s,%(Pantry_After_Opening_Metric)s,
          %(Refrigerate_Min)s,%(Refrigerate_Max)s,%(Refrigerate_Metric)s,%(Refrigerate_tips)s,
          %(DOP_Refrigerate_Min)s,%(DOP_Refrigerate_Max)s,%(DOP_Refrigerate_Metric)s,%(DOP_Refrigerate_tips)s,
          %(Refrigerate_After_Opening_Min)s,%(Refrigerate_After_Opening_Max)s,%(Refrigerate_After_Opening_Metric)s,
          %(Refrigerate_After_Thawing_Min)s,%(Refrigerate_After_Thawing_Max)s,%(Refrigerate_After_Thawing_Metric)s,
          %(Freeze_Min)s,%(Freeze_Max)s,%(Freeze_Metric)s,%(Freeze_Tips)s,
          %(DOP_Freeze_Min)s,%(DOP_Freeze_Max)s,%(DOP_Freeze_Metric)s,%(DOP_Freeze_Tips)s
        );
        """
    )

    count = 0
    with conn.cursor() as cur:
        for raw in products:
            rec = normalize_record(raw)
            # Ensure all expected keys are present (None if missing)
            keys = [
                "ID","Category_ID","Name","Name_subtitle","Keywords","Pantry_Min","Pantry_Max","Pantry_Metric","Pantry_tips",
                "DOP_Pantry_Min","DOP_Pantry_Max","DOP_Pantry_Metric","DOP_Pantry_tips",
                "Pantry_After_Opening_Min","Pantry_After_Opening_Max","Pantry_After_Opening_Metric",
                "Refrigerate_Min","Refrigerate_Max","Refrigerate_Metric","Refrigerate_tips",
                "DOP_Refrigerate_Min","DOP_Refrigerate_Max","DOP_Refrigerate_Metric","DOP_Refrigerate_tips",
                "Refrigerate_After_Opening_Min","Refrigerate_After_Opening_Max","Refrigerate_After_Opening_Metric",
                "Refrigerate_After_Thawing_Min","Refrigerate_After_Thawing_Max","Refrigerate_After_Thawing_Metric",
                "Freeze_Min","Freeze_Max","Freeze_Metric","Freeze_Tips",
                "DOP_Freeze_Min","DOP_Freeze_Max","DOP_Freeze_Metric","DOP_Freeze_Tips",
            ]
            params = {k: rec.get(k) for k in keys}
            cur.execute(insert_sql, params)
            count += 1
    return count


def main() -> None:
    data = read_json("product.json")
    products = data.get("data", [])

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
        inserted = upsert_items(conn, products)
        print(f"Processed {inserted} items (existing IDs ignored).")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
