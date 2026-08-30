local modifier_prefixes = { 'Alt-', 'C-', 'M-', 'A-', 'S-', 'D-' }

local mod_map = {
  ['C-'] = 'Control',
  ['M-'] = 'Alt',
  ['A-'] = 'Alt',
  ['Alt-'] = 'Alt',
  ['S-'] = 'Shift',
  ['D-'] = 'Meta',
}

local function upper_letter(s)
  if #s == 1 and s:match('%a') then
    return s:upper()
  end
  return s
end

local function normalize_key(key)
  if #key == 1 then
    return upper_letter(key)
  end

  if key == 'Space' or key == 'SPC' or key == 'spc' then
    return 'Space'
  end
  if key == 'CR' or key == 'Return' or key == 'Enter' then
    return 'Return'
  end
  if key == 'Tab' or key == 'TAB' then
    return 'Tab'
  end
  if key == 'Esc' or key == 'Escape' then
    return 'Esc'
  end

  return key
end

local function consume_prefix(inner)
  for _, prefix in ipairs(modifier_prefixes) do
    if inner:sub(1, #prefix) == prefix then
      return prefix, inner:sub(#prefix + 1)
    end
  end
  return nil, inner
end

local function parse_bracket(token)
  local inner = token:match('^<(.*)>$')
  if not inner then
    return nil
  end

  local mods = {}
  while true do
    local prefix
    prefix, inner = consume_prefix(inner)
    if not prefix then
      break
    end
    table.insert(mods, mod_map[prefix])
  end

  if inner == '' then
    error('empty key inside bracket token ' .. token)
  end

  return {
    kind = 'chord',
    mods = table.concat(mods, '|'),
    key = normalize_key(inner),
  }
end

local function parse_token(token)
  if token:sub(1, 1) == '<' and token:sub(-1) == '>' then
    return parse_bracket(token)
  end

  return { kind = 'plain', key = normalize_key(token) }
end

local function tokenize(wire)
  local tokens = {}
  for token in vim.gsplit(vim.fn.trim(wire), ' ', true) do
    if token ~= '' then
      table.insert(tokens, token)
    end
  end
  return tokens
end

local function wire_to_steps(wire)
  local steps = {}
  for _, token in ipairs(tokenize(wire)) do
    table.insert(steps, parse_token(token))
  end
  return steps
end

local raw_argv = vim.fn.argv(0)
local wire = os.getenv('QUARRY_ORACLE_WIRE')
if not wire or wire == '' then
  wire = type(raw_argv) == 'table' and table.concat(raw_argv, ' ') or (raw_argv or '')
end

io.write(vim.json.encode({ wire = wire, steps = wire_to_steps(wire) }))
io.flush()
vim.cmd('qa!')
