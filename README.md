# Ensek Technical Test

By Andrew Harman

## Contents

* [Original brief](docs/original-brief.md)

## Usage

```
curl -i --data-binary @Meter_Reading.csv -H "Content-Type: text/csv" "https://localhost:7006/meter-reading-uploads?isValidationOnlyMode=false"
```

## Implementation

I'm trying to implement a version that would allow files containing between 100,000 and 1,000,000 rows to be uploaded in a timely manner
(those were the types of sizes I used to see with Powergen).

So the approach I'm using (with SQL Server as the database) is:

* Convert the CSV record stream to a table-valued parameter stream (so I can stream all of the readings in a single database stored procedure call without a huge memory footprint).
* Use a stored procedure to bulk-validate readings, and bulk insert into the target table.
* Validation is broken into 2 parts:
    * The initial data-type validation within the streaming pipeline.
    * Database-based validation (valid account, duplicate detection) - which is delegated to the database.
* It's not spectacularly object-oriented, but it will be very fast and scalable.

My acceptance criteria:

* Similar memory profile for 10,000 and 1,000,000 row files (demonstrates scalability).
    * Memory starts up in 18.2Mb.
    * Meter_Reading.csv takes the memory up to 23Mb.
    * First run of Test100000.csv takes the memory up to 198Mb.
    * Second run of Test100000.csv takes the memory up to 345Mb.
    * Every subsequent run leavs memory at 345Mb.
    * First run of Test200000.csv takes the memory up to 365Mb.
    * CPU runs at 2% for the WebApp (the SQL Server runs considerably hotter - as expected).

* Able to import 100,000 rows within 20 seconds running locally.
  * Test100000.csv takes approximately 5 seconds.
  * Test200000.csv takes approximately 6 seconds.

## License

MIT license, see [License](LICENSE.md).
