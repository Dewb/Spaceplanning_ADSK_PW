#!/usr/bin/env python

import togo
import argparse
import pickle

def build_togo(specifications_filename):
    specfile = open(specifications_filename, 'rb')
    togo_specs = pickle.load(specfile)
    return togo.Togo(togo_specs)

def write_output(spaces, output_filename):
    result = togo.service.TogoServiceResult(spaces)
    outfile = open(output_filename, 'wb')
    pickle.dump(result, outfile)

def get_cli():
    parser = argparse.ArgumentParser(description="Togo space optimization")
    parser.add_argument("--specifications", "-s", dest="specifications_filename", help="Filename with specifications in JSON format")
    parser.add_argument("--output", "-o", dest="output_filename", help="Filename to store the JSON results")
    args = parser.parse_args()
    return args

def run(args):
    engine = build_togo(args.specifications_filename)
    spaces = engine.run()
    write_output(spaces, args.output_filename)

if __name__ == "__main__":
    args = get_cli()
    run(args)
