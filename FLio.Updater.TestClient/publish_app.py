import argparse
import os
import xml.etree.ElementTree as xmlp
import re
import shutil
from pathlib import Path
from typing import Optional


def increment_assembly_version(inc_build: bool, inc_minor: bool, inc_major: bool, set_version: Optional[str]) -> str:
    if (inc_build or inc_minor or inc_major) is False and set_version is None:
        inc_build = True

    file = [f for f in os.listdir(".") if "csproj" in f][0]
    tree = xmlp.parse(file)
    root = tree.getroot()
    property_group_element: Optional[xmlp.Element] = None

    for child in root:
        if child.tag == 'PropertyGroup':
            property_group_element = child
            break
    version_element = property_group_element.find('AssemblyVersion')

    version_split = version_element.text.split(".") if version_element is not None else []
    (major, minor, build) = (int(i) for i in version_split) if version_element is not None else (1, 0, 0)

    # increment only if previous version found
    if version_element is not None:
        if inc_major:
            major += 1
            minor = 0
            build = 0
        if inc_minor:
            minor += 1
            build = 0
        if inc_build:
            build += 1

    # override previous versions if set_version is specified
    if set_version is not None:
        (major, minor, build) = set_version.split(".")

    if version_element is None:
        version_element = xmlp.SubElement(property_group_element, 'AssemblyVersion')

    version_string = ".".join([str(i) for i in [major, minor, build]])
    version_element.text = version_string
    tree.write(file)
    return version_element.text


def increment_assembly_version_o(inc_build: bool, inc_minor: bool, inc_major: bool, set_version: Optional[str]) -> str:
    if (inc_build or inc_minor or inc_major) is False and set_version is None:
        inc_build = True

    file = [f for f in os.listdir(".") if "csproj" in f][0]
    content = open(file).read()
    matches = re.findall("<AssemblyVersion>(\d+)\.(\d+)\.(\d+)", content)

    (major, minor, build) = (int(i) for i in matches[0]) if len(matches) > 0 else (1, 0, 0)
    if set_version is not None:
        (major, minor, build) = set_version.split(".")

    # increment only if previous version found
    if len(matches) > 0:
        if inc_major:
            major += 1
            minor = 0
            build = 0
        if inc_minor:
            minor += 1
            build = 0
        if inc_build:
            build += 1

    version_string = ".".join([str(i) for i in [major, minor, build]])
    new_content = re.sub(
        "<AssemblyVersion>(\d+\.\d+\.\d+)",
        f"<AssemblyVersion>{version_string}",
        content,
    )
    open(file, "w").write(new_content)
    return version_string


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("-b", "--build", type=bool, default=False)
    parser.add_argument("-m", "--minor", type=bool, default=False)
    parser.add_argument("-M", "--major", type=bool, default=False)
    parser.add_argument("--version", type=str, default=None)
    args = parser.parse_args()

    version = increment_assembly_version(args.build, args.minor, args.major, args.version)
    os.system("dotnet build --configuration Release")
    Path("dist").mkdir(parents=True, exist_ok=True)
    shutil.make_archive(f"dist/{version}", "zip", "bin/Release/net6.0")


if __name__ == "__main__":
    main()
