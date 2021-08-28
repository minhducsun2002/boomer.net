import os
import sys
import tarfile
import time
import io
from pathlib import Path
from urllib import request
from dotenv import load_dotenv
from parse_all_datavals import main

load_dotenv()

url = os.environ['TEST_LIST_URL']
print(f"Loading tests from list at {url}")

with request.urlopen(url) as file:
    files: str = file.read().decode('utf-8')

for line in files.splitlines():
    version, url, *_ = line.split(' = ')
    print(f"=> {version}")
    path = os.path.join(sys.argv[1], "master", version)
    if not Path(path).exists():
        print("   Downloading...")
        # download_path = os.path.join(sys.argv[1], f"{version}.tar.gz")
        # request.urlretrieve(url, download_path)
        file = request.urlopen(url)

        print(f"   Extracting to {path}...")
        Path(path).mkdir(parents=True, exist_ok=True)
        tar = tarfile.open(fileobj=file, mode="r:gz")
        tar.extractall(path)
        print("   Extraction complete.")
    else:
        print(f"   Data folder present. Skipping download.")

    print("   Parsing data...")
    start_time = time.perf_counter()
    main(path, path)
    end_time = time.perf_counter()
    print(f"   Time elapsed    : {end_time - start_time:.4f}s\n")
