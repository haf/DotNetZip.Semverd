use strict;
use IO::Compress::Zip qw(:all);

my $dir = $ARGV[1];
opendir(DIR, $dir) or die $!;

my @files = ();

while (my $file = readdir(DIR)) {

  # We only want files
  next unless (-f "$dir/$file");

  # Use a regular expression to find files ending in .txt
  next unless ($file =~ m/^[^\.]/);

  push(@files, "$dir\\$file");
}

closedir(DIR);


##my @files = grep { /^[^\.]/ } readdir(DIR);

foreach my $file2 (@files) {
  print "$file2\n";
}


zip \@files => $ARGV[0]
  or die "Cannot create zip file: $ZipError\n";
