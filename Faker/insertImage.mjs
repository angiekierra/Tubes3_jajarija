import fs from "fs";
import csv from "csv-parser";
import mysql from "mysql";
import { config } from "dotenv";

// Load environment variables from .env file
config();
// Create a database connection
const connection = mysql.createConnection({
  host: process.env.DB_HOST,
  user: process.env.DB_USER,
  password: process.env.DB_PASSWORD,
  database: process.env.DB_DATABASE,
});
// Connect to the database
connection.connect((err) => {
  if (err) {
    console.error("Error connecting to database:", err);
    return;
  }
  console.log("Connected to database");

  // Delete old data from the table
  const deleteSql = "DELETE FROM sidik_jari";

  connection.query(deleteSql, (err, result) => {
    if (err) {
      console.error("Error deleting old data:", err);
      connection.end();
      return;
    }
    console.log("Old data deleted successfully");

    // Read the CSV file
    const results = [];
    let count = 0; // Variable to track the number of records processed

    // Read the CSV file
    fs.createReadStream("person_image.csv")
      .pipe(csv())
      .on("data", (data) => {
        // Check if the count exceeds 100
        // if (count >= 200) {
        //   return; // Skip further processing
        // }

        results.push(data);
        count++; // Increment the count
      })
      .on("end", () => {
        // Prepare SQL insert statement
        const tableName = "sidik_jari";
        const columns = Object.keys(results[0]).join(",");
        const placeholders = results
          .map(
            () =>
              "(" +
              Object.keys(results[0])
                .map(() => "?")
                .join(",") +
              ")"
          )
          .join(",");
        const values = results.map((obj) => Object.values(obj)).flat();

        const sql = `INSERT INTO ${tableName} (${columns}) VALUES ${placeholders}`;

        // Insert the data into the database
        connection.query(sql, values, (err, result) => {
          if (err) {
            console.error("Error inserting data:", err);
            connection.end();
            return;
          }
          console.log("Data inserted successfully");

          // Close the connection
          connection.end();
        });
      });
  });
});
