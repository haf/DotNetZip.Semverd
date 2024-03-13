root_folder = File.expand_path("#{File.dirname(__FILE__)}/..")
require "buildscripts/project_details"

# The folders array denoting where to place build artifacts

folders = {
  :root => root_folder,
  :src => "src",
  :build => "build",
  :binaries => "placeholder - environment.rb sets this depending on target",
  :tools => "tools",
  :tests => "build/tests",
  :nuget => "build/nuget",
  :nuspec => "build/nuspec"
}

FOLDERS = folders.merge({

  :bzip => {
      :test_dir => 'BZip2 Tests',
      :nuspec => "#{File.join(folders[:nuspec], PROJECTS[:bzip][:nuget_key])}",
      :out => 'placeholder - environment.rb will sets this',
      :test_out => 'placeholder - environment.rb sets this'
  },
  
  :zip => {
      :test_dir => '',
      :nuspec => "#{File.join(folders[:nuspec], PROJECTS[:zip][:nuget_key])}",
      :out => 'placeholder - environment.rb will sets this',
      :test_out => 'placeholder - environment.rb sets this'
  },
  
  :zipfull => {
      :test_dir => '',
      :nuspec => "#{File.join(folders[:nuspec], PROJECTS[:zipfull][:nuget_key])}",
      :out => 'placeholder - environment.rb will sets this',
      :test_out => 'placeholder - environment.rb sets this'
  },
  
  :zipred => {
      :test_dir => '',
      :nuspec => "#{File.join(folders[:nuspec], PROJECTS[:zipred][:nuget_key])}",
      :out => 'placeholder - environment.rb will sets this',
      :test_out => 'placeholder - environment.rb sets this'
  },
  
  :zlib => {
      :test_dir => 'Zlib Tests',
      :nuspec => "#{File.join(folders[:nuspec], PROJECTS[:zlib][:nuget_key])}",
      :out => 'placeholder - environment.rb will sets this',
      :test_out => 'placeholder - environment.rb sets this'
  },
  
})

FILES = {
  :sln => "src/DotNetZip.sln",
  
  :bzip => {
    :nuspec => File.join(FOLDERS[:bzip][:nuspec], "#{PROJECTS[:bzip][:nuget_key]}.nuspec")
  },
  
  :zip => {
    :nuspec => File.join(FOLDERS[:zip][:nuspec], "#{PROJECTS[:zip][:nuget_key]}.nuspec")
  },
  
  :zipfull => {
    :nuspec => File.join(FOLDERS[:zipfull][:nuspec], "#{PROJECTS[:zipfull][:nuget_key]}.nuspec")
  },
  
  :zipred => {
    :nuspec => File.join(FOLDERS[:zipred][:nuspec], "#{PROJECTS[:zipred][:nuget_key]}.nuspec")
  },
  
  :zlib => {
    :nuspec => File.join(FOLDERS[:zlib][:nuspec], "#{PROJECTS[:zlib][:nuget_key]}.nuspec")
  },
  
}

COMMANDS = {
  :nuget => File.join(FOLDERS[:tools], "NuGet.exe"),
  :ilmerge => File.join(FOLDERS[:tools], "ILMerge.exe")
  # nunit etc
}

URIS = {
  :nuget_offical => "http://packages.nuget.org/v1/",
  :nuget_symbolsource => "http://nuget.gw.symbolsource.org/Public/Nuget"
}