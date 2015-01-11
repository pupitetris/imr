package Cmd::Image;

use Image::Magick;
use File::Spec;

require 'CHARP-cmd.pm';

%PERSONA_THUMB_RESIZE = (width => 128, height => 128);

sub CreatePersonaThumb {
    my ($cmd, $num_parms, $parms, $fcgi) = @_;

    if ($num_parms != 1) {
	return { 'err' => 'CGI:CMDNUMPARAM', 'parms' => [ $cmd, 1, $num_parms ] };
    }

    if ($fcgi->param ('file') eq undef) {
	return CHARP::cmderr ($cmd, 'Missing file data.');
    }

    my $path = CHARP::file_find (File::Spec->catfile ('thumbs', $parms->[0]));
    return $path if $path->{'err'};

    my $err = CHARP::dir_create ($cmd, $path);
    return $err if $err;

    my $image = Image::Magick->new (magick => 'jpg');
    $image->BlobToImage ($fcgi->param ('file'));
    my ($w, $h) = $image->Get('width', 'height');
    if ($w > $h) {
	$image->Crop (x => ($w - $h) / 2, y => 0, width => $h, height => $h);
    } elsif ($h > $w) {
	$image->Crop (x => 0, y => ($h - $w) / 2, width => $w, height => $w);
    }
    $image->Thumbnail (%PERSONA_THUMB_RESIZE);
    $image->Write ($path->{'fname'});

    return undef;
}

1;

