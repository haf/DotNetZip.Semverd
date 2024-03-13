require "buildscripts/paths"
require "buildscripts/project_details"
require 'semver'

namespace :env do

  task :common do
    # version management
    fv = version SemVer.find.to_s
    revision = (!fv[3] || fv[3] == 0) ? (ENV['BUILD_NUMBER'] || Time.now.strftime('%j%H')) : fv[3] #  (day of year 0-265)(hour 00-24)

    ENV['BUILD_VERSION'] = BUILD_VERSION = "#{ SemVer.new(fv[0], fv[1], fv[2]).format "%M.%m.%p" }.#{revision}"
    puts "Assembly Version: #{BUILD_VERSION}."
    puts "##teamcity[buildNumber '#{BUILD_VERSION}']" # tell teamcity our decision

    # .net/mono configuration management
    ENV['FRAMEWORK'] = FRAMEWORK = ENV['FRAMEWORK'] || (Rake::Win32::windows? ? "net40" : "mono28")
    puts "Framework: #{FRAMEWORK}"
  end

  # configure the output directories
  task :configure, [:str] do |_, args|
    ENV['CONFIGURATION'] = CONFIGURATION = args[:str]
    FOLDERS[:binaries] = File.join(FOLDERS[:build], FRAMEWORK, args[:str].downcase)
    CLEAN.include(File.join(FOLDERS[:binaries], "*"))
  end

  task :set_dirs do


    FOLDERS[:bzip][:out] = File.join(FOLDERS[:src], PROJECTS[:bzip][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:bzip][:out])

    # for tests
    FOLDERS[:bzip][:test_out] = File.join(FOLDERS[:src], PROJECTS[:bzip][:test_dir], 'bin', CONFIGURATION)
    FILES[:bzip][:test] = File.join(FOLDERS[:bzip][:test_out], "#{PROJECTS[:bzip][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])


    FOLDERS[:zip][:out] = File.join(FOLDERS[:src], PROJECTS[:zip][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:zip][:out])

    # for tests
    FOLDERS[:zip][:test_out] = File.join(FOLDERS[:src], PROJECTS[:zip][:test_dir], 'bin', CONFIGURATION)
    FILES[:zip][:test] = File.join(FOLDERS[:zip][:test_out], "#{PROJECTS[:zip][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])


    FOLDERS[:zipfull][:out] = File.join(FOLDERS[:src], PROJECTS[:zipfull][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:zipfull][:out])

    # for tests
    FOLDERS[:zipfull][:test_out] = File.join(FOLDERS[:src], PROJECTS[:zipfull][:test_dir], 'bin', CONFIGURATION)
    FILES[:zipfull][:test] = File.join(FOLDERS[:zipfull][:test_out], "#{PROJECTS[:zipfull][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])


    FOLDERS[:zipred][:out] = File.join(FOLDERS[:src], PROJECTS[:zipred][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:zipred][:out])

    # for tests
    FOLDERS[:zipred][:test_out] = File.join(FOLDERS[:src], PROJECTS[:zipred][:test_dir], 'bin', CONFIGURATION)
    FILES[:zipred][:test] = File.join(FOLDERS[:zipred][:test_out], "#{PROJECTS[:zipred][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])


    FOLDERS[:zlib][:out] = File.join(FOLDERS[:src], PROJECTS[:zlib][:dir], 'bin', CONFIGURATION)
    CLEAN.include(FOLDERS[:zlib][:out])

    # for tests
    FOLDERS[:zlib][:test_out] = File.join(FOLDERS[:src], PROJECTS[:zlib][:test_dir], 'bin', CONFIGURATION)
    FILES[:zlib][:test] = File.join(FOLDERS[:zlib][:test_out], "#{PROJECTS[:zlib][:test_dir]}.dll")
    CLEAN.include(FOLDERS[:test_out])

  end

  task :dir_tasks do
    all_dirs = []

    [:build, :tools, :tests, :nuget, :nuspec].each do |dir|
      directory FOLDERS[dir]
      all_dirs <<  FOLDERS[dir]
    end

    [:out, :nuspec, :test_out].each do |dir|
      [:bzip, :zip, :zipfull, :zipred, :zlib].each{ |k|
        directory FOLDERS[k][dir]
        all_dirs << FOLDERS[k][dir]
      }
    end

    all_dirs.each do |d|
      Rake::Task[d].invoke
    end
  end

  # DEBUG/RELEASE

  desc "set debug environment variables"
  task :debug => [:common] do
    Rake::Task["env:configure"].invoke('Debug')
    Rake::Task["env:set_dirs"].invoke
    Rake::Task["env:dir_tasks"].invoke
  end

  desc "set release environment variables"
  task :release => [:common] do
    Rake::Task["env:configure"].invoke('Release')
    Rake::Task["env:set_dirs"].invoke
    Rake::Task["env:dir_tasks"].invoke
  end

  # FRAMEWORKS

  desc "set net40 framework"
  task :net40 do
    ENV['FRAMEWORK'] = 'net40'
  end

  desc "set net35 framework"
  task :net35 do
    ENV['FRAMEWORK'] = 'net35'
  end

  desc "set net20 framework"
  task :net20 do
    ENV['FRAMEWORK'] = 'net20'
  end

  desc "set mono28 framework"
  task :mono28 do
    ENV['FRAMEWORK'] = 'mono28'
  end

  desc "set mono30 framework"
  task :net30 do
    ENV['FRAMEWORK'] = 'mono30'
  end

  # ENVIRONMENT VARS FOR PRODUCT RELEASES

  desc "set GA envionment variables"
  task :ga do
    puts "##teamcity[progressMessage 'Setting environment variables for GA']"
    ENV['OFFICIAL_RELEASE'] = OFFICIAL_RELEASE = "4000"
  end

  desc "set release candidate environment variables"
  task :rc, [:number] do |t, args|
    puts "##teamcity[progressMessage 'Setting environment variables for Release Candidate']"
    arg_num = args[:number].to_i
    num = arg_num != 0 ? arg_num : 1
    ENV['OFFICIAL_RELEASE'] = OFFICIAL_RELEASE = "#{3000 + num}"
  end

  desc "set beta-environment variables"
  task :beta, [:number] do |t, args|
    puts "##teamcity[progressMessage 'Setting environment variables for Beta']"
    arg_num = args[:number].to_i
    num = arg_num != 0 ? arg_num : 1
    ENV['OFFICIAL_RELEASE'] = OFFICIAL_RELEASE = "#{2000 + num}"
  end

  desc "set alpha environment variables"
  task :alpha, [:number] do |t, args|
    puts "##teamcity[progressMessage 'Setting environment variables for Alpha']"
    arg_num = args[:number].to_i
    num = arg_num != 0 ? arg_num : 1

    ENV['OFFICIAL_RELEASE'] = OFFICIAL_RELEASE = "#{1000 + num}"
  end
end
