import { faker } from "@faker-js/faker";
import json2csv from "json2csv";
import fs from "fs";
import path from "path";

// Function to generate NIK
function generateNIK(
  provinceCode,
  cityCode,
  subdistrictCode,
  birthDate,
  gender
) {
  provinceCode = provinceCode.toString().padStart(2, "0");
  cityCode = cityCode.toString().padStart(2, "0");
  subdistrictCode = subdistrictCode.toString().padStart(2, "0");

  const dd = birthDate.getDate().toString().padStart(2, "0");
  const mm = (birthDate.getMonth() + 1).toString().padStart(2, "0");
  const yy = birthDate.getFullYear().toString().slice(-2);

  let ddFormatted = dd;
  if (gender.toLowerCase() === "perempuan") {
    ddFormatted = (parseInt(dd) + 40).toString();
  }

  const dateOfBirth = `${ddFormatted}${mm}${yy}`;
  const serialNumber = Math.floor(1000 + Math.random() * 9000).toString();
  const nik = `${provinceCode}${cityCode}${subdistrictCode}${dateOfBirth}${serialNumber}`;

  return nik;
}

// Function to create dummy persons based on the filenames
function createDummyPerson(folderPath) {
  const person = [];
  const person_image = [];
  const enum_finger_index = [
    "Left_index_finger",
    "Left_little_finger",
    "Left_middle_finger",
    "Left_ring_finger",
    "Left_thumb_finger",
    "Right_index_finger",
    "Right_little_finger",
    "Right_middle_finger",
    "Right_ring_finger",
    "Right_thumb_finger",
  ];

  const files = fs.readdirSync(folderPath).sort((a, b) => {
    const aIndex = a.split("_")[0];
    const bIndex = b.split("_")[0];
    return aIndex - bIndex;
  });

  // Process each file to create person and person_image entries
  files.forEach((file, index) => {
    const match = file.match(
      /^(\d+)__(M|F)_(Left|Right)_(index|middle|ring|little|thumb)_finger\.BMP$/
    );
    if (match) {
      const [, personIndex, gender, hand, finger] = match;
      const genderFull = gender === "M" ? "Laki-laki" : "Perempuan";

      if (!person[personIndex]) {
        const birthDate = faker.date.birthdate({
          min: 18,
          max: 65,
          mode: "age",
        });
        person[personIndex] = {
          NIK: generateNIK(32, 73, 4, birthDate, genderFull),
          nama: faker.person.fullName(),
          tempat_lahir: faker.location.city(),
          tanggal_lahir: birthDate.toISOString().split("T")[0],
          jenis_kelamin: genderFull,
          golongan_darah: faker.helpers.arrayElement(["A", "B", "AB", "O"]),
          alamat: faker.location.streetAddress(),
          agama: faker.helpers.arrayElement([
            "Islam",
            "Kristen",
            "Katolik",
            "Hindu",
            "Buddha",
            "Kong Hu Cu",
          ]),
          status_perkawinan: faker.helpers.arrayElement([
            "Belum Menikah",
            "Menikah",
            "Cerai",
          ]),
          pekerjaan: faker.person.jobTitle(),
          kewarganegaraan: "Indonesia",
        };
      }

      person_image.push({
        berkas_citra: file,
        nama: person[personIndex].nama,
      });
    } else {
      console.log(`File does not match pattern: ${file}`);
    }
  });

  return { person: person.filter(Boolean), person_image };
}

// Specify the path to the folder containing the images
const folderPath = "./test/Real";

const { person, person_image } = createDummyPerson(folderPath);

const personCSV = json2csv.parse(person);
const personImageCSV = json2csv.parse(person_image);

fs.writeFileSync("person.csv", personCSV);
fs.writeFileSync("person_image.csv", personImageCSV);

console.log("CSV files have been successfully generated.");
