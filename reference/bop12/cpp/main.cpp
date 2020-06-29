#include <iostream>
#include <string>
#include <cstdlib>
#include <ctime>
#include <fstream>
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
	
	cbop::Polygon subj, clip;
	if (! subj.open (argv[1])) {
		std::string fileError = std::string (argv[1]) + " does not exist or has a bad format\n";
		fatalError (fileError, 3);
	}
	if (! clip.open (argv[2])) {
		std::string fileError = std::string (argv[2]) + " does not exist or has a bad format\n";
		fatalError (fileError, 3);
	}
	// The parameters are correct
	cbop::BooleanOpType op = cbop::INTERSECTION;

	if (argc > 3) {
		switch (argv[3][0]) {
			case 'U':
				op = cbop::UNION;
				break;
			case 'D':
				op = cbop::DIFFERENCE;
				break;
			case 'X':
				op = cbop::XOR;
				break;
		}
	}

	cbop::Polygon result;
	clock_t start = clock ();
	cbop::compute (subj, clip, result, op);
	clock_t stop = clock ();
	std::cout << (stop - start) / double (CLOCKS_PER_SEC) << " seconds\n";
//	std::cout << result;
	return 0;
}
