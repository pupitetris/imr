#!/usr/bin/perl

# Mime-type exporter, processes source of:
# http://www.freeformatter.com/mime-types-list.html

$id = 1; # mime_type_id

$col = 0; # column number
while (<>) {
    chomp;
    if (/^<tr>$/) {
	print $id . '|';
    }
    if (/^<td>/) {
	s/<\/?td>//g;
	if ($col == 2) {
	    s/^\.//;
	}
	if ($col == 3) {
	    s/<\/?a[^>]*>//g; # Remove anchor code, keep text content only.
	}
	print if $col < 3; # print except beyond 3rd column (the more info column).
	print '|' if $col < 2;
	$col ++;
    }
    if (/^<\/tr>$/) {
	$id++;
	$col = 0;
	print "\n";
    }
}
