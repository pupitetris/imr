#!/usr/bin/perl

use IO::Handle;
use POSIX qw(:termios_h);

$COMPORT = $ARGV[0];

open $com, '+<', $COMPORT or die $!;
#$com->blocking(0);

$term = POSIX::Termios->new;
$term->setiflag ($term->getiflag &
				 (&POSIX::IGNBRK | &POSIX::IGNPAR &
				  ~&POSIX::INPCK & ~&POSIX::IXON &
				  ~&POSIX::IXOFF)); # No special processing.
$term->setlflag ($term->getlflag &
				 ~(&POSIX::ICANON | &POSIX::ECHO |
				   &POSIX::ECHONL | &POSIX::ISIG |
				   &POSIX::IEXTEN)); # Raw mode.

$cflag = $term->getcflag;
$cflag &= ~&POSIX::CSIZE;
$cflag |= ~&POSIX::CS8;
$cflag &= ~&POSIX::PARENB;
$cflag &= ~&POSIX::CSTOPB;
$term->setcflag ($cflag); # 8N1

$term->setospeed (&POSIX::B2400);
$term->setispeed (&POSIX::B2400);
$term->setattr (fileno ($com), $POSIX::TCSANOW) or die $!;

%PROTO = (
	'W' => ['284a-er45-FG34-09%#-12w+q', 1], # Init string, sleep 1 sec.
	'N' => '', # Send frequency (Prepare) -- Analysis
	'T' => '', # Send frequency (Done) -- Analysis
	'S' => ['532', 1], # <AUTOSIMILE>
	'Y' => ['533', 1], # <COPY>
	'P' => ['534', 1], # <IMPRINT>
	'I' => ['535', 1], # Erasing Data stop -- Telecuracion
	'D' => ['536', 1], # Erasing Data
	'V' => ['538', 1], # <SAVE> Guardar en tarjeta
	'E' => ['539', 1], # <ERASE>
	);
		  

while ($com->sysread ($c, 1)) {
	$sleep = 0;

	print STDERR "<< $c\n";

	if (!exists $PROTO{$c}) {
		print STDERR "!! Unrecognized command\n";
		next;
	} else {
		$res = $PROTO{$c};
		if (ref $res == 'ARRAY') {
			sleep ($res->[1]);
			$res = $res->[0];
		}

		if ($res ne '') {
			print STDERR ">> $res\n";
			$com->syswrite ($res);
			$com->flush ();
		}
	}
}


print STDERR "Done.\n";
