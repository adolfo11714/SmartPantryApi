-- Create database if not exists and switch to it
CREATE DATABASE IF NOT EXISTS `food_pantry` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `food_pantry`;

-- Create items table
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

-- Create categories table
CREATE TABLE IF NOT EXISTS `categories` (
  `ID` INT NOT NULL,
  `Category_Name` VARCHAR(255) NOT NULL,
  `Subcategory_Name` VARCHAR(255) NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Seed Butter row
INSERT INTO `items` (
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
  1, 7, 'Butter', NULL, 'Butter', NULL, NULL, NULL, 'May be left at room temperature for 1 - 2 days.',
  NULL, NULL, NULL, NULL,
  NULL, NULL, NULL,
  NULL, NULL, NULL, NULL,
  1, 2, 'Months', NULL,
  NULL, NULL, NULL,
  NULL, NULL, NULL,
  NULL, NULL, NULL, NULL,
  6, 9, 'Months', NULL
);
