Add-Type -AssemblyName System.Data
$connStr = "Server=db75937.public.databaseasp.net; Database=db75937; User Id=db75937; Password=Mo@123456; Encrypt=True; TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT Id, InstructorId, Status, StartsAtUtc, EndsAtUtc FROM Sessions WHERE Id = 'FE6E02B2-5395-4156-A6B0-2DE5B07782AF'"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    $id = $reader["Id"]
    $status = $reader["Status"]
    $start = $reader["StartsAtUtc"]
    $end = $reader["EndsAtUtc"]
    Write-Output "ID: $id | Status: $status | Start: $start | End: $end"
}
$conn.Close()

