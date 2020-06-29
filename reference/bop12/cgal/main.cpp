#include <iostream>
#include <ctime>
#include "booleanop.h"

void fatalError (const std::string& message, int exitCode)
{
	std::cerr << message;
	exit (exitCode);
}

int main (int argc, char* argv[])
{
	std::string paramError = "Syntax: " + std::string (argv[0]) + " subject clipping [I|U|D|X]\n";
	paramError += "\tThe last parameter is optional. It can be I (Intersection), U (Union), D (Difference) or X (eXclusive or)\n";
	paramError += "\tThe last parameter default value is I\n";
	if (argc < 3)
		fatalError (paramError, 1);
	const std::string ope = "IUDX";
	if (argc > 3 && ope.find (argv[3][0]) == std::string::npos)
		fatalError (paramError, 2);
	
	bop::Polygon subj, clip;
	if (! subj.open (argv[1])) {
		std::string fileError = std::string (argv[1]) + " does not exist or has a bad format\n";
		fatalError (fileError, 3);
	}
	if (! clip.open (argv[2])) {
		std::string fileError = std::string (argv[2]) + " does not exist or has a bad format\n";
		fatalError (fileError, 3);
	}
	bop::BooleanOpType op = bop::INTERSECTION;
	if (argc > 3) {
		switch (argv[3][0]) {
			case 'U':
				op = bop::UNION;
				break;
			case 'D':
				op = bop::DIFFERENCE;
				break;
			case 'X':
				op = bop::XOR;
				break;
		}
	}
	bop::Polygon result;
	clock_t start = clock ();
	bop::compute (subj, clip, result, op);
	clock_t stop = clock ();
	std::cout << (stop - start) / double (CLOCKS_PER_SEC) << " seconds\n";
//	std::cout << result;
	return 0;
}
