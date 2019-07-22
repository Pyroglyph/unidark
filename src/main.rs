extern crate subslice;
use crate::subslice::SubsliceExt;
use std::env;
use std::fs;
use std::process;
use std::io::BufWriter;
use std::fs::File;
use std::io::Write;

fn main() {
    // Constants
    let theme_byte = 0x74;
    let magic_sequence = vec![0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3];

    let args: Vec<String> = env::args().collect();

    if args.len() != 2 {
        println!("Wrong number of arguments. Expected 1 argument pointing to the Unity executable.");
        process::exit(1);
    }

    let filename = &args[1];
    println!("Loading {}", filename);
    match fs::read(filename) {
        Ok(file) => {
            println!("Loaded {} bytes.", file.len());
            println!("Processing...");

            match file.find(&magic_sequence) {
                Some(seq_index) => {
                    let byte_index = seq_index + magic_sequence.len();

                    let mut new_file = file;
                    new_file[byte_index] = theme_byte;

                    match replace_file(filename, &new_file, true) {
                        Ok(_) => {
                            println!("Success!");
                            process::exit(0);
                        },
                        Err(x) => {
                            eprintln!("Error: {}", x);
                            process::exit(1);
                        }
                    };
                },
                None => {
                    eprintln!("Offset not found. The file has not been changed.");
                    process::exit(1);
                }
            }
        }
        Err(e) => eprintln!("Something went wrong reading the file:\n{}", e),
    };
}

fn replace_file(path: &str, file: &Vec<u8>, backup: bool) -> std::io::Result<()> {
    if backup {
        fs::rename(path, path.to_owned() + ".bak")?;
    }

    let mut buffer = BufWriter::new(File::create(path)?);

    buffer.write_all(file)?;
    buffer.flush()?;
    Ok(())
}
