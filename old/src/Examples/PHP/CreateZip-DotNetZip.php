<?php

try
{
echo '<html>';
echo '  <head>';
echo '  <title>Calling .NET from PHP through COM</title>';
echo '  <link rel="stylesheet" href="basic.css"/>';
echo '  </head>';
echo '<body>';

  echo '<h2>Hello!</h2>' . "<br/>\n";
  echo '<h4>Trying static method</h4>' . "<br/>\n";
  $fname = "archive-" . date('Y-m-d-His') . ".zip";
  echo 'Dynamically generated archive name: ' . "\n" . '<h4>' . $fname . "</h4>\n";

  $zipOutput = "c:\\temp\\php-" . $fname;
  $zipfact = new COM("Ionic.Zip.ZipFile");
  $zip = $zipfact->Read($zipOutput);
  $zip->Encryption = 3;
  $zip->Password = "AES-Encryption-Is-Secure";

  $dirToZip= "c:\\temp\\psh";
  $zip->AddDirectory($dirToZip);
  $zip->Save();

  echo '<br/>The file was saved to ' . $zip->Name . '<br/>' . "\n";

  $zip->Dispose();

  echo '</body>';
  echo '</html>';

} 
catch (Exception $e) {
  echo 'Caught exception: ',  $e->getMessage(), "\n";
  echo '<pre>';
  echo $e->getTraceAsString(), "\n";
  echo '</pre>';
}


?>
