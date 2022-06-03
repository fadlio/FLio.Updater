import json
import os
import re
import shutil
from pathlib import Path


def get_application_name():
    with open('updater.json') as json_file:
        data = json.load(json_file)
        return data['General']['ApplicationName']


def modify_csproject(application_name: str):
    file = [f for f in os.listdir(".") if "csproj" in f][0]
    content = open(file).read()

    new_content = re.sub('<AssemblyName>.+<\/AssemblyName>', f'<AssemblyName>{application_name}.Loader</AssemblyName>',
                         content)
    open('temp.csproj', "w").write(new_content)


def main():
    application_name = get_application_name()
    modify_csproject(application_name)
    os.system("dotnet build -c Release temp.csproj")
    Path("dist").mkdir(parents=True, exist_ok=True)
    shutil.make_archive(f"dist/{application_name}.Loader", "zip", "bin/Release/net6.0")
    os.remove("temp.csproj")


if __name__ == "__main__":
    main()
