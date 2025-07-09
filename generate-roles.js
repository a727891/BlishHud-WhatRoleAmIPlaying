const puppeteer = require('puppeteer');
const fs = require('fs-extra');

const OUTPUT_FILE = './roles.json';
const SNOWCROWS_URL = 'https://snowcrows.com/builds/raids';
const OUTPUT_PATH = require('path').resolve(__dirname, '/roles.json');

// Profession mapping from URL to display name
const PROFESSION_MAP = {
  'guardian': 'Guardian',
  'warrior': 'Warrior', 
  'engineer': 'Engineer',
  'ranger': 'Ranger',
  'thief': 'Thief',
  'elementalist': 'Elementalist',
  'mesmer': 'Mesmer',
  'necromancer': 'Necromancer',
  'revenant': 'Revenant'
};

// Profession metadata
const PROFESSIONS = [
  { "name": "Guardian", "id": 1, "icon": "156633" },
  { "name": "Warrior", "id": 2, "icon": "156642" },
  { "name": "Engineer", "id": 3, "icon": "156631" },
  { "name": "Ranger", "id": 4, "icon": "156639" },
  { "name": "Thief", "id": 5, "icon": "103581" },
  { "name": "Elementalist", "id": 6, "icon": "156629" },
  { "name": "Mesmer", "id": 7, "icon": "156635" },
  { "name": "Necromancer", "id": 8, "icon": "156637" },
  { "name": "Revenant", "id": 9, "icon": "965717" }
];

// Elite spec metadata
const ELITE_SPECS = [
  { "name": "Dragonhunter", "id": 27, "profession": "Guardian", "icon": "1128572", "background": "1012076" },
  { "name": "Firebrand", "id": 62, "profession": "Guardian", "icon": "1770210", "background": "2479309" },
  { "name": "Willbender", "id": 68, "profession": "Guardian", "icon": "2479351", "background": "2491509" },
  { "name": "Berserker", "id": 18, "profession": "Warrior", "icon": "1128566", "background": "1012110" },
  { "name": "Spellbreaker", "id": 61, "profession": "Warrior", "icon": "1770222", "background": "2479310" },
  { "name": "Bladesworn", "id": 70, "profession": "Warrior", "icon": "2491563", "background": "2491512" },
  { "name": "Scrapper", "id": 43, "profession": "Engineer", "icon": "1128580", "background": "1012042" },
  { "name": "Holosmith", "id": 57, "profession": "Engineer", "icon": "1770224", "background": "2479311" },
  { "name": "Mechanist", "id": 74, "profession": "Engineer", "icon": "2503656", "background": "2503611" },
  { "name": "Druid", "id": 5, "profession": "Ranger", "icon": "1128574", "background": "1012077" },
  { "name": "Soulbeast", "id": 55, "profession": "Ranger", "icon": "1770214", "background": "2479312" },
  { "name": "Untamed", "id": 72, "profession": "Ranger", "icon": "2503660", "background": "2503612" },
  { "name": "Daredevil", "id": 7, "profession": "Thief", "icon": "1128570", "background": "1012101" },
  { "name": "Deadeye", "id": 58, "profession": "Thief", "icon": "1770212", "background": "2479313" },
  { "name": "Specter", "id": 71, "profession": "Thief", "icon": "2503664", "background": "2503613" },
  { "name": "Tempest", "id": 48, "profession": "Elementalist", "icon": "1128582", "background": "1012075" },
  { "name": "Weaver", "id": 56, "profession": "Elementalist", "icon": "1670505", "background": "2479314" },
  { "name": "Catalyst", "id": 67, "profession": "Elementalist", "icon": "2491555", "background": "2491510" },
  { "name": "Chronomancer", "id": 40, "profession": "Mesmer", "icon": "1128568", "background": "1012081" },
  { "name": "Mirage", "id": 59, "profession": "Mesmer", "icon": "1770216", "background": "2479315" },
  { "name": "Virtuoso", "id": 69, "profession": "Mesmer", "icon": "2479355", "background": "2491511" },
  { "name": "Reaper", "id": 34, "profession": "Necromancer", "icon": "1128578", "background": "1012082" },
  { "name": "Scourge", "id": 60, "profession": "Necromancer", "icon": "1770220", "background": "2479316" },
  { "name": "Harbinger", "id": 75, "profession": "Necromancer", "icon": "2479359", "background": "2503617" },
  { "name": "Herald", "id": 52, "profession": "Revenant", "icon": "1128576", "background": "1012083" },
  { "name": "Renegade", "id": 63, "profession": "Revenant", "icon": "1770218", "background": "2479317" },
  { "name": "Vindicator", "id": 76, "profession": "Revenant", "icon": "2491559", "background": "2491511" }
];

function determineRoleType(roleText, buildName) {
  const text = (roleText + ' ' + buildName).toLowerCase();
  
  if (text.includes('heal')) return 'Healer';
  return 'DPS';
}

function determineBoonProvision(roleText, buildName) {
  const text = (roleText + ' ' + buildName).toLowerCase();
  
  // Special case for boon chronomancer
  if (text.includes('boon chronomancer')) {
    return { providesQuickness: true, providesAlacrity: true };
  }
  
  const providesQuickness = text.includes('quickness');
  const providesAlacrity = text.includes('alacrity');
  return { providesQuickness, providesAlacrity };
}

(async () => {
  console.log('Starting Snow Crows roles generator...');
  
  const browser = await puppeteer.launch();
  const page = await browser.newPage();

  console.log(`Navigating to ${SNOWCROWS_URL} ...`);
  await page.goto(SNOWCROWS_URL, { waitUntil: 'networkidle2' });

  console.log('Scrolling to load all builds...');
  await autoScroll(page);
  console.log('Scrolling complete.');

  console.log('Waiting for build links...');
  await page.waitForSelector('a[href^="/builds/"]');
  console.log('Build links loaded.');

  // Extract builds
  const builds = await page.evaluate(() => {
    return Array.from(document.querySelectorAll('a[href^="/builds/"]')).map(card => {
      const buildUrl = card.href.startsWith('http') ? card.href : `https://snowcrows.com${card.getAttribute('href')}`;
      const h2 = card.querySelector('h2');
      const build_name = h2 ? h2.textContent.trim() : '';
      
      // Role from colored span
      let role = '';
      const roleSpan = card.querySelector('span.bg-indigo-900\\/30, span.bg-red-900\\/30, span.bg-green-900\\/30, span.bg-yellow-900\\/30');
      if (roleSpan) {
        role = roleSpan.textContent.trim();
      }
      
      // Extract weapons from build name (e.g., "Heal Quickness Scrapper (Shortbow)" → "Shortbow")
      let weapons = '';
      const weaponMatch = build_name.match(/\(([^)]+)\)$/);
      if (weaponMatch) {
        weapons = weaponMatch[1].trim();
      }
      
      return {
        build_url: buildUrl,
        build_name,
        role,
        weapons
      };
    });
  });

  console.log(`Found ${builds.length} build links. Processing...`);

  // Filter out incomplete entries and transform to expected format
  console.table(builds);
  const roles = builds
    .filter(b => b.build_url && b.build_name && b.role)
    .map(build => {
      // Find profession and elite spec by matching against the build name
      let profession = '';
      let elite_spec = '';
      
      // Try to match elite spec first (more specific)
      for (const spec of ELITE_SPECS) {
        if (build.build_name.toLowerCase().includes(spec.name.toLowerCase())) {
          elite_spec = spec.name;
          profession = spec.profession;
          break;
        }
      }
      
      // If no elite spec found, try to match base profession
      if (!profession) {
        for (const prof of PROFESSIONS) {
          if (build.build_name.toLowerCase().includes(prof.name.toLowerCase())) {
            profession = prof.name;
            break;
          }
        }
      }
      
      const roleType = determineRoleType(build.role, build.build_name);
      const { providesQuickness, providesAlacrity } = determineBoonProvision(build.role, build.build_name);
      const description = build.build_name;
      
      return {
        profession,
        elite_spec,
        role: build.role,
        role_type: roleType,
        provides_quickness: providesQuickness,
        provides_alacrity: providesAlacrity,
        description,
        weapons: build.weapons,
        build_url: build.build_url
      };
    })
    //.filter(role => role.profession && role.elite_spec); // Only include roles where we successfully matched both profession and elite spec

  // Create the final JSON structure
  const outputData = {
    version: "0.1.0",
    last_updated: new Date().toISOString(),
    professions: PROFESSIONS,
    elite_specs: ELITE_SPECS,
    roles: roles
  };

  console.log(`Writing ${roles.length} roles to ${OUTPUT_FILE} ...`);
  await fs.writeJson(OUTPUT_FILE, outputData, { spaces: 2 });

  console.log('Done!');
  await browser.close();
})();

async function autoScroll(page){
  console.log('Auto-scrolling page...');
  await page.evaluate(async () => {
    await new Promise((resolve) => {
      var totalHeight = 0;
      var distance = 100;
      var timer = setInterval(() => {
        var scrollHeight = document.body.scrollHeight;
        window.scrollBy(0, distance);
        totalHeight += distance;
        if(totalHeight >= scrollHeight){
          clearInterval(timer);
          resolve();
        }
      }, 200);
    });
  });
  console.log('Auto-scroll finished.');
}