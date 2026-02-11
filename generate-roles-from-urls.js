#!/usr/bin/env node
/**
 * Generates role definitions for roles.json from a text file of Snow Crows raid build URLs.
 * Adds a `beginner` flag for builds whose URL slug starts with "beginner-".
 *
 * Usage:
 *   node generate-roles-from-urls.js [urls-file] [--output roles.json]
 *
 * Example:
 *   node generate-roles-from-urls.js ./snowcrows_raid_build_urls.txt
 *   node generate-roles-from-urls.js C:\Users\Soeed\Downloads\snowcrows_raid_build_urls.txt --output roles.json
 */

const fs = require('fs');
const path = require('path');

const DEFAULT_URLS_FILE = path.join(__dirname, 'snowcrows_raid_build_urls.txt');
const DEFAULT_OUTPUT = path.join(__dirname, 'roles.json');

// Profession: URL slug -> display name
const PROFESSION_MAP = {
  guardian: 'Guardian',
  warrior: 'Warrior',
  engineer: 'Engineer',
  ranger: 'Ranger',
  thief: 'Thief',
  elementalist: 'Elementalist',
  mesmer: 'Mesmer',
  necromancer: 'Necromancer',
  revenant: 'Revenant',
};

// Elite spec display names; order by length descending so longer names match first (e.g. Dragonhunter before Hunter)
const ELITE_SPEC_NAMES = [
  'Dragonhunter', 'Spellbreaker', 'Bladesworn', 'Firebrand', 'Willbender',
  'Holosmith', 'Mechanist', 'Soulbeast', 'Daredevil', 'Chronomancer',
  'Virtuoso', 'Harbinger', 'Renegade', 'Vindicator', 'Luminary',
  'Amalgam', 'Galeshot', 'Antiquary', 'Troubadour', 'Ritualist',
  'Conduit', 'Tempest', 'Catalyst', 'Scourge', 'Reaper', 'Herald',
  'Scrapper', 'Druid', 'Untamed', 'Mirage', 'Evoker', 'Specter',
  'Berserker', 'Paragon', 'Weaver', 'Deadeye',
].sort((a, b) => b.length - a.length);

function parseUrl(url) {
  const trimmed = url.trim();
  if (!trimmed || !trimmed.startsWith('http')) return null;
  const match = trimmed.match(/snowcrows\.com\/builds\/raids\/([^/]+)\/([^/?#]+)/);
  if (!match) return null;
  const [, professionSlug, buildSlug] = match;
  const profession = PROFESSION_MAP[professionSlug.toLowerCase()];
  if (!profession) return null;
  return { profession, buildSlug, url: trimmed };
}

function titleCase(str) {
  return str.replace(/\b\w/g, (c) => c.toUpperCase());
}

function formatWeapons(parts) {
  if (!parts || parts.length === 0) return '';
  return parts.map((p) => titleCase(p.replace(/-/g, ' '))).join(' / ');
}

function parseBuildSlug(buildSlug) {
  const parts = buildSlug.split('-').filter(Boolean);
  let beginner = false;
  let start = 0;
  if (parts[0] === 'beginner') {
    beginner = true;
    start = 1;
  }
  const rest = parts.slice(start);

  // Find elite spec: first segment that matches an elite spec name (case-insensitive)
  let specIndex = -1;
  let eliteSpec = '';
  for (const spec of ELITE_SPEC_NAMES) {
    const slugPart = spec.toLowerCase();
    const idx = rest.findIndex((p) => p === slugPart);
    if (idx >= 0) {
      specIndex = idx;
      eliteSpec = spec;
      break;
    }
  }

  // If no elite spec found, check for base profession as segment (e.g. power-warrior-mace-axe)
  if (specIndex < 0) {
    const baseProfessions = ['guardian', 'warrior', 'engineer', 'ranger', 'thief', 'elementalist', 'mesmer', 'necromancer', 'revenant'];
    const idx = rest.findIndex((p) => baseProfessions.includes(p));
    if (idx >= 0) {
      specIndex = idx;
      eliteSpec = titleCase(rest[idx]);
    }
  }
  if (specIndex < 0) return null;

  const prefix = rest.slice(0, specIndex);
  const weaponParts = rest.slice(specIndex + 1);
  if (!eliteSpec) eliteSpec = titleCase(rest[specIndex]);

  const lowerPrefix = prefix.join(' ').toLowerCase();
  const hasHeal = lowerPrefix.includes('heal');
  const hasAlacrity = lowerPrefix.includes('alacrity') || lowerPrefix.includes('boon');
  const hasQuickness = lowerPrefix.includes('quickness') || lowerPrefix.includes('boon');

  let role = 'DPS';
  if (hasHeal) role = 'Healer';
  else if (lowerPrefix.includes('condition')) role = 'Condition';
  else if (lowerPrefix.includes('power')) role = 'Power';
  else if (lowerPrefix.includes('celestial')) role = 'Healer';
  else if (lowerPrefix.includes('inferno')) role = 'Power';

  const roleType = hasHeal || role === 'Healer' ? 'Healer' : 'DPS';

  // Description: title-case prefix + elite spec + (weapons)
  const descParts = prefix.map((p) => titleCase(p));
  if (beginner) descParts.unshift('Beginner');
  descParts.push(eliteSpec);
  const description = weaponParts.length
    ? `${descParts.join(' ')} (${formatWeapons(weaponParts)})`
    : descParts.join(' ');

  const weapons = formatWeapons(weaponParts);

  return {
    beginner,
    eliteSpec,
    role,
    roleType,
    provides_quickness: hasQuickness,
    provides_alacrity: hasAlacrity,
    description,
    weapons,
    prefix,
    weaponParts,
  };
}

function buildRoleFromUrl(parsedUrl, eliteSpecToProfession) {
  const { profession, buildSlug, url } = parsedUrl;
  const parsed = parseBuildSlug(buildSlug);
  if (!parsed) return null;

  const { eliteSpec, role, roleType, provides_quickness, provides_alacrity, description, weapons, beginner } = parsed;

  return {
    profession,
    elite_spec: eliteSpec,
    role,
    role_type: roleType,
    provides_quickness: provides_quickness,
    provides_alacrity: provides_alacrity,
    description,
    weapons,
    build_url: url,
    beginner: !!beginner,
  };
}

function main() {
  const args = process.argv.slice(2);
  let urlsPath = DEFAULT_URLS_FILE;
  let outputPath = DEFAULT_OUTPUT;

  for (let i = 0; i < args.length; i++) {
    if (args[i] === '--output' && args[i + 1]) {
      outputPath = path.resolve(args[i + 1]);
      i++;
    } else if (!args[i].startsWith('--')) {
      urlsPath = path.resolve(args[i]);
    }
  }

  if (!fs.existsSync(urlsPath)) {
    console.error('URLs file not found:', urlsPath);
    process.exit(1);
  }

  const urlsContent = fs.readFileSync(urlsPath, 'utf8');
  const urls = urlsContent
    .split(/\r?\n/)
    .map((u) => u.trim())
    .filter(Boolean);

  // Build elite spec -> profession from known list (must match roles.json elite_specs)
  const eliteSpecToProfession = {};
  const rolesJsonPath = path.join(__dirname, 'roles.json');
  if (fs.existsSync(rolesJsonPath)) {
    const existing = JSON.parse(fs.readFileSync(rolesJsonPath, 'utf8'));
    (existing.elite_specs || []).forEach((s) => {
      eliteSpecToProfession[s.name] = s.profession;
    });
  }

  const roles = [];
  const skipped = [];
  for (const url of urls) {
    const parsed = parseUrl(url);
    if (!parsed) {
      skipped.push(url);
      continue;
    }
    const role = buildRoleFromUrl(parsed, eliteSpecToProfession);
    if (!role) {
      skipped.push(url);
      continue;
    }
    roles.push(role);
  }

  console.log(`Parsed ${roles.length} role definitions from ${urls.length} URLs.`);
  if (skipped.length) {
    console.warn(`Skipped ${skipped.length} URLs:`);
    skipped.forEach((u) => console.warn('  ', u));
  }

  const beginnerCount = roles.filter((r) => r.beginner).length;
  console.log(`Beginner builds: ${beginnerCount}`);

  // Load existing roles.json to preserve professions and elite_specs
  let output = {
    version: '0.2.0',
    last_updated: new Date().toISOString(),
    professions: [],
    elite_specs: [],
    roles,
  };

  if (fs.existsSync(outputPath)) {
    const existing = JSON.parse(fs.readFileSync(outputPath, 'utf8'));
    output.professions = existing.professions || output.professions;
    output.elite_specs = existing.elite_specs || output.elite_specs;
    if (existing.version) output.version = existing.version;
  }

  fs.writeFileSync(outputPath, JSON.stringify(output, null, 2), 'utf8');
  console.log(`Wrote ${roles.length} roles to ${outputPath}`);
}

main();
